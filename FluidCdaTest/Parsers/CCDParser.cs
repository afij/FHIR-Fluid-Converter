using Fluid;
using Fluid.Ast;
using Fluid.Values;
using FluidCdaTest.Filters;
using FluidCdaTest.Models;
using Parlot.Fluent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace FluidCdaTest.Parsers
{
    // Tags are written here because they sometimes rely on protected FluidParser variables
    public class CCDParser : FluidParser
    {
        /// <summary>
        /// Create CCDParser. Automatically registers required liquid tags
        /// </summary>
        public CCDParser()
        {
            RegisterCustomTags();
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
                Primary.And(Separated(Literals.Char(' ').Or(Literals.Char(',')), Identifier.AndSkip(Colon).And(Primary).Then(x => {
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
                using (StreamReader reader = new StreamReader(templateInfo.CreateReadStream()))
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
                tempOptions.Filters.RegisterCustomFilters();

                var output = template.Render(new TemplateContext(new Dictionary<string, object> { { "obj", valueFromExpression } }, tempOptions));
                var assignmentStatement = new AssignStatement(evaluateObj.Variable, new LiteralExpression(new StringValue(output.Trim())));
                await assignmentStatement.WriteToAsync(w, e, c);
                return Completion.Normal;
            });
        }
    }
}
