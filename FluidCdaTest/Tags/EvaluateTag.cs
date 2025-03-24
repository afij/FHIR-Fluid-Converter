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

public static class EvaluateTag 
{
    internal static void RegisterEvaluateTag(this FluidParser parser)
    {
        Parser<EvaluateStruct> EvaluateParser = Terms.NonWhiteSpace().AndSkip(Terms.Text("using")).And(Terms.String(StringLiteralQuotes.Single))
                .AndSkip(Terms.Text("obj:")).And(Terms.NonWhiteSpace())
                .Then<EvaluateStruct>(v =>
                {
                    return new EvaluateStruct() { Parser = parser, Variable = v.Item1.ToString(), Template = v.Item2.ToString(), InputObjectString = v.Item3.ToString() };
                });

        parser.RegisterParserTag("evaluate", EvaluateParser, async (evaluateObj, w, e, c) =>
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
                throw new Fluid.ParseException($"Null template contents: {evaluateObj.Template}.liquid");
            }

            if (!evaluateObj.Parser.TryParse(templateContent, out var template, out var errors))
            {
                throw new Fluid.ParseException(errors);
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