using System.Reflection;
using Microsoft.Extensions.Localization;

namespace TP_CSharp.Models
{
    public class LanguageService
    {
        private readonly IStringLocalizer _localizer;

        public LanguageService(IStringLocalizerFactory factory)
        {
            var type = typeof(ChangeLang);
            var assemblyFullName = type.GetTypeInfo().Assembly.FullName;
            if (assemblyFullName == null) return;
            var assemblyName = new AssemblyName(assemblyFullName);
            if (assemblyName.Name != null) _localizer = factory.Create("SharedResource", assemblyName.Name);
        }

        public LocalizedString GetKey(string key)
        {
            return _localizer[key];
        }
    }
}
