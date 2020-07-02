using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain.Code
{
    public class AssemblySvTimeStampAttribute : Attribute
    {
        public string Timestamp;
    }

    public class CustomAssembly
    {
        public static string GetAssemblySvTimeStamp()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblySvTimeStampAttribute svAttr =
                    (AssemblySvTimeStampAttribute)Attribute.GetCustomAttribute(
                    asm, typeof(AssemblySvTimeStampAttribute));
            if(svAttr != null)
                return svAttr.Timestamp;
            else
                return "No Timestamp";
        }
    }
}
