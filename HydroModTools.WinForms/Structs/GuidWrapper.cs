﻿using HydroModTools.WinForms.Converters;
using System;
using System.ComponentModel;

namespace HydroModTools.WinForms.Structs
{
    [TypeConverter(typeof(GuiWrapperConverter))]
    internal struct GuidWrapper
    {
        private Guid guidValue;

        public GuidWrapper(Guid guid)
        {
            guidValue = guid;
        }

        public GuidWrapper(string guid)
        {
            try
            {
                guidValue = new Guid(guid);
            }
            catch
            {
                guidValue = Guid.Empty;
            }
        }


        public static implicit operator GuidWrapper(string guid)
        {
            return new GuidWrapper(guid);
        }

        public override string ToString()
        {
            return guidValue.ToString("N").ToUpper();
        }

        public Guid GetGuid()
        {
            return guidValue;
        }
    }
}
