using System;

namespace ThinRL.Core
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class StopDecodeIfNotEqualAttribute : Attribute
    {
        public readonly object CheckValue;
        public StopDecodeIfNotEqualAttribute(object checkValue)
        {
            this.CheckValue = checkValue;
        }
    }
}
