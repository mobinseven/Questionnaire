using BlazorBoilerplate.Shared.Helpers;
using Breeze.Sharp;
using System;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class DbLog : BaseEntity
    {

        public Int32 Id
        {
            get { return GetValue<Int32>(); }
            set { SetValue(value); }
        }

        public String Exception
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String Level
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String Message
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String MessageTemplate
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String Properties
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public DateTime TimeStamp
        {
            get { return GetValue<DateTime>(); }
            set { SetValue(value); }
        }
        public IDictionary<string, string> LogProperties
        {
            get
            {
                return RegexUtilities.DirtyXmlPropertyParser(Properties);
            }
        }

    }
}
