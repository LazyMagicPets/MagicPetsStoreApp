using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorUI;

public static class AssemblyContent
{
    /// <summary>
    /// Read an assembly embedded resource. 
    /// Use the form: "FolderName/FileName.ext". 
    /// The routine will replace the / with . and prepend the assembly name 
    /// to conform to the GetManifestResourceStream() requirements.
    /// The content being retrieved must be identified as an embedded resource 
    /// in the project file.
    /// </summary>
    /// <param name="embeddedResource"></param>
    /// <returns></returns>
    public static string ReadEmbeddedResource(string embeddedResource)
    {
        var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;
        var assemblyName = assembly!.GetName().Name;
        try
        {
            var resourceName = $"{assemblyName}/{embeddedResource}".Replace("/", ".");
            using var messagesStream = assembly.GetManifestResourceStream(resourceName)!;
            if (messagesStream != null)
            {
                using var messagesReader = new StreamReader(messagesStream);
                var messagesText = messagesReader.ReadToEnd();
                return messagesText;
            }
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "";
        }   
    }

}
