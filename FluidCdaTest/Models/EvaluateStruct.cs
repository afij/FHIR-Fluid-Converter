using Fluid;
using Fluid.Ast;

namespace FluidCdaTest.Models
{
    public struct EvaluateStruct
    {
        public FluidParser Parser { get; set; }
        public string Variable { get; set; }
        public string Template { get; set; }
        public Expression ObjExpression { get; set; }
        public string InputObjectString { get; set; }
    }
}
