using System;

namespace Malong.Common.CustomizedAttribute
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDescriptionAttribute : System.Attribute
    {
        public string Text { get; set; }

        public EnumDescriptionAttribute(string text)
            : this()
        {
            this.Text = text;
        }

        public EnumDescriptionAttribute()
            : base()
        {

        }
    }
}
