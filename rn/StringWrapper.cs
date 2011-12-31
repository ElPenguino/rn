using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rn
{
    class StringWrapper : IFormattable
    {
        private String value;
        public StringWrapper(String input)
        {
            this.value = input;
        }
        public string ToString(string format)
        {
            return this.ToString(format, null);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return value;
            if (String.IsNullOrEmpty(format)) format = "SS";
            if ((format == "SS") || (format == "S"))
                return this.value;
            Char basetype = format[0];
            Char finaltype = format[1];
            format = format.Trim().ToUpperInvariant();
            var tmpval = false;
            switch (finaltype)
            {
                case 'H':
                    return String.Format("", tmpval);
                default:
                    throw new FormatException(String.Format("The '{0}' format string is not supported.", finaltype));
            }
        }
    }
}
