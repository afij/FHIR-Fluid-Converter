﻿using Fhir.Fluid.Converter.Filters;
using Fhir.Fluid.Converter.Models;
using Fhir.Fluid.Converter.Parsers.Options;
using Fhir.Fluid.Converter.Processors;
using Fhir.Fluid.Converter.Providers;
using Fhir.Fluid.Converter.Utilities;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Parlot.Fluent.Parsers;

namespace Fhir.Fluid.Converter.Parsers
{
    // Tags are written here because they sometimes rely on protected FluidParser variables
    internal class CCDParser : FluidParser
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
            RegisterCustomFluidTags();

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

            LoadCodeMappings();
        }

        /// <summary>
        /// Register custom tags for CCD parsing
        /// </summary>
        private void RegisterCustomFluidTags()
        {
            RegisterIncludeTag();
            RegisterEvaluateTag();
        }

        /// <summary>
        /// Parse ValueSet CodeMapping object and assign to instance variable
        /// </summary>
        private void LoadCodeMappings()
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
                .Then(v =>
                {
                    return new EvaluateStruct() { Parser = this, Variable = v.Item1.ToString(), Template = v.Item2.ToString(), InputObjectString = v.Item3.ToString() };
                });

            RegisterParserTag("evaluate", EvaluateParser, async (evaluateObj, w, e, c) =>
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
                var memberSegments = evaluateObj.InputObjectString.Split('.')
                    .Select(x => new IdentifierSegment(x))
                    .ToList();
                var memberExpression = new MemberExpression(memberSegments);
                var valueFromExpression = await memberExpression.EvaluateAsync(c);

                // Reuse existing _templateOptions
                var output = template.Render(new TemplateContext(new Dictionary<string, object> { { "obj", valueFromExpression } }, _templateOptions));
                var assignmentStatement = new AssignStatement(evaluateObj.Variable, new LiteralExpression(new StringValue(output.Trim())));
                await assignmentStatement.WriteToAsync(w, e, c);
                return Completion.Normal;
            });
        }

        /// <summary>
        /// Parse and Render a CCDA string into a FHIR string
        /// </summary>
        /// <param name="inputCCDA"></param>
        /// <returns></returns>
        public async Task<string> ConvertCcdaToFhirAsync(string inputCCDA)
        {
            IFluidTemplate template = ParseRootTemplate();
            return await RenderTemplateAsync(template, inputCCDA);
        }

        /// <summary>
        /// Manually parse the RootTemplate into an IFluidTemplate
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IFluidTemplate ParseRootTemplate()
        {
            // Load model from disk
            _rootTemplateContent ??= _fileProvider.ReadTemplateFile(_options.RootTemplate);

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

        /// <summary>
        /// Render the specified IFluidTemplate with an string CCDA
        /// </summary>
        /// <param name="template">IFluidTemplate to be rendered</param>
        /// <param name="inputCCDA">String CCDA to be converted</param>
        /// <returns></returns>
        public async Task<string> RenderTemplateAsync(IFluidTemplate template, string inputCCDA)
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
