using Fluid;
using Fluid.Ast;
using Fluid.Values;
using FluidCdaTest.Filters;
using FluidCdaTest.Models;
using FluidCdaTest.Parsers.Options;
using FluidCdaTest.Processors;
using FluidCdaTest.Providers;
using FluidCdaTest.Utilities;
using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Parlot.Fluent.Parsers;

namespace FluidCdaTest.Parsers
{
    // Tags are written here because they sometimes rely on protected FluidParser variables
    public class CCDParser : FluidParser
    {
        private readonly CCDParserOptions _options;
        private readonly TemplateOptions _templateOptions;
        private readonly ICDAFileProvider _fileProvider;
        private string _rootTemplateContent = null;
        private CodeMapping _codeMapping;

        /// <summary>
        /// Create CCDParser with a dedicated FileProvider. Automatically reigsters filters
        /// </summary>
        public CCDParser(CCDParserOptions options)
        {
            _options = options;
            RegisterCustomTags();

            // Create TemplateOptions and register filters and custom provider
            _templateOptions = new TemplateOptions();
            _templateOptions.Filters.RegisterCustomFilters(this);
            if (_options.UseCachedFileProvider)
            {
                _fileProvider = new CachedCDAFileProvider(_options.TemplateDirectoryPath);
            }
            else
            {
                _fileProvider = new CDAFileProvider(_options.TemplateDirectoryPath);
            }

            _templateOptions.FileProvider = _fileProvider;

            ParseCodeMapping();
        }

        /// <summary>
        /// Register custom tags for CCD parsing
        /// </summary>
        private void RegisterCustomTags()
        {
            RegisterIncludeTag();
            RegisterEvaluateTag();
        }

        /// <summary>
        /// Parse ValueSet CodeMapping object and assign to instance variable
        /// </summary>
        private void ParseCodeMapping()
        {
            // Preload ValueSet data as CodeMapping obj
            var valueSetString = _fileProvider.ReadTemplateFile(@"ValueSet/ValueSet");
            _codeMapping = TemplateUtility.ParseCodeMapping(valueSetString);
        }

        /// <summary>
        /// Registers/Overrides default Fluid Include tag
        /// </summary>
        /// Included directly on parser as it accesses protected variables
        private void RegisterIncludeTag()
        {
            // Have to register Include tag here due to most of the expressions being protected in FluidParser
            var IncludeTag = OneOf(
                Primary.AndSkip(Comma).And(Separated(Comma, Identifier.AndSkip(Colon).And(Primary).Then(x => new AssignStatement(x.Item1, x.Item2)))).Then(x => new IncludeStatement(this, x.Item1, null, null, null, x.Item2)),
                //Primary.And(Separated(Comma, Identifier.AndSkip(Colon).And(Primary).Then(x =>
                //{
                //    return new AssignStatement(x.Item1, x.Item2);
                //}))).Then(x => new IncludeStatement(this, x.Item1, null, null, null, x.Item2)),
                Primary.And(Separated(Literals.Char(' ').Or(Literals.Char(',')), Identifier.AndSkip(Colon).And(Primary).Then(x =>
                {
                    return new AssignStatement(x.Item1, x.Item2);
                }))).Then(x => new IncludeStatement(this, x.Item1, null, null, null, x.Item2)),
                Primary.AndSkip(Terms.Text("with")).And(Primary).And(ZeroOrOne(Terms.Text("as").SkipAnd(Identifier))).Then(x => new IncludeStatement(this, x.Item1, with: x.Item2, alias: x.Item3)),
                Primary.AndSkip(Terms.Text("for")).And(Primary).And(ZeroOrOne(Terms.Text("as").SkipAnd(Identifier))).Then(x => new IncludeStatement(this, x.Item1, @for: x.Item2, alias: x.Item3)),
                Primary.Then(x => new IncludeStatement(this, x))
                ).AndSkip(TagEnd)
                .Then<Statement>(x => x)
                .ElseError("Invalid 'include' tag")
                ;

            // Override existing tag
            RegisteredTags["include"] = IncludeTag;
        }

        /// <summary>
        /// Registers the Evaluate tag
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ParseException"></exception>
        private void RegisterEvaluateTag()
        {
            Parser<EvaluateStruct> EvaluateParser = Terms.NonWhiteSpace().AndSkip(Terms.Text("using")).And(Terms.String(StringLiteralQuotes.Single))
                .AndSkip(Terms.Text("obj:")).And(Terms.NonWhiteSpace())
                .Then<EvaluateStruct>(v =>
                {
                    return new EvaluateStruct() { Parser = this, Variable = v.Item1.ToString(), Template = v.Item2.ToString(), InputObjectString = v.Item3.ToString() };
                });

            this.RegisterParserTag("evaluate", EvaluateParser, async (evaluateObj, w, e, c) =>
            {
                var templateFileSystem = c.Options.FileProvider;
                var templateInfo = templateFileSystem.GetFileInfo($"{evaluateObj.Template}.liquid");

                if (templateInfo == null || !templateInfo.Exists)
                {
                    throw new FileNotFoundException($"{evaluateObj.Template}.liquid");
                }

                string templateContent = null;
                using (StreamReader reader = new(templateInfo.CreateReadStream()))
                {
                    templateContent = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrEmpty(templateContent))
                {
                    throw new ParseException($"Null template contents: {evaluateObj.Template}.liquid");
                }

                if (!evaluateObj.Parser.TryParse(templateContent, out var template, out var errors))
                {
                    throw new ParseException(errors);
                }

                // Parse string to get input object's member expression
                var splitInputObject = evaluateObj.InputObjectString.Split('.');
                List<MemberSegment> inputMemberSegments = splitInputObject.Select(x => new IdentifierSegment(x)).ToList().Cast<MemberSegment>().ToList();
                var memberExpression = new MemberExpression(inputMemberSegments);
                var valueFromExpression = await memberExpression.EvaluateAsync(c);

                // Construct temp options with registered tags
                var tempOptions = new TemplateOptions();
                tempOptions.Filters.RegisterCustomFilters(this);

                var output = template.Render(new TemplateContext(new Dictionary<string, object> { { "obj", valueFromExpression } }, tempOptions));
                var assignmentStatement = new AssignStatement(evaluateObj.Variable, new LiteralExpression(new StringValue(output.Trim())));
                await assignmentStatement.WriteToAsync(w, e, c);
                return Completion.Normal;
            });
        }

        public IFluidTemplate Parse()
        {
            // Load model from disk
            //var testModel  = await File.ReadAllTextAsync(@"C:\work\FluidCdaTest\FluidCdaTest\testModel.txt");
            _rootTemplateContent ??= _fileProvider.ReadTemplateFile(_options.RootTemplate);
            //var rootTemplate = "{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}{% include 'Test' test: 'testxd' test2: msg -%}";
            //var rootTemplate = "{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}{% include 'Header' test: testval, test2: testval -%}";
            //var rootTemplate = "{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}{% include 'Header', test: testval, test2: testval2, t3: t3 -%}";
            //var rootTemplate = "{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}{% include 'Header' test: testval, test2: testval2, t3: t3 -%}";
            //var rootTemplate = @"{% evaluate patientId using 'Utils/GenerateId' obj: msg.ClinicalDocument.recordTarget.patientRole -%}
            //value: {{ patientId }}";

            // Parse the template
            if (this.TryParse(_rootTemplateContent, out var template, out var errors))
            {
                return template;
            }
            else
            {
                throw new Exception("Failed to parse template: " + string.Join(", ", errors));
            }
        }

        public async Task<string> RenderAsync(IFluidTemplate template, string inputCCDA)
        {
            // Process model into an object (fixes data etc) and add to a new context
            var preProcessedObject = PreProcessor.ParseToObject(inputCCDA);
            var context = new TemplateContext(new Dictionary<string, object> { { "msg", preProcessedObject } }, _templateOptions);

            context.AmbientValues.Add(GeneralFilters.CODE_MAPPING_VALUE_NAME, _codeMapping);

            var renderedString = await template.RenderAsync(context);
            var mergedJsonString = PostProcessor.Process(renderedString);

            return mergedJsonString;
        }
    }
}
