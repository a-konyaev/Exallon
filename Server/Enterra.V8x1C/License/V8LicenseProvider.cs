using System;
using System.ComponentModel;

namespace Enterra.V8x1C.License
{
    internal class V8LicenseProvider : LicenseProvider 
    {
        public override System.ComponentModel.License GetLicense(
            LicenseContext context,
            Type type,
            object instance,
            bool allowExceptions
            )
        {

            if (type == typeof (DOM.Session))
            {
                return new V8License();
            }

            return null;
        }

        private class V8License : System.ComponentModel.License
        {
            public override void Dispose()
            {}

            public override string LicenseKey
            {
                get { return null; }
            }
        }
    }
}
