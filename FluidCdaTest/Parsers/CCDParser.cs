using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace FluidCdaTest.Parsers
{
    public class CCDParser : FluidParser
    {
        //public Deferred<Expression> PrimaryParser => Primary;

        /// <summary>
        /// Register custom tags for CCD parsing
        /// </summary>
        public void RegisterCustomTags()
        {
            RegisterIncludeTag();
            this.RegisterEvaluateTag();
        }

        /// <summary>
        /// Registers/Overrides default Fluid Include tag
        /// </summary>
        /// Included directly on parser as it accesses protected variables
        public void RegisterIncludeTag()
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
    }
}
