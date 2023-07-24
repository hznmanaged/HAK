using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.AppointmentKeeper.Services
{
    public class FontService
    {
        readonly IEnumerable<FontFamily> Families;
        public const string DefaultFont = "Segoe UI";

        public FontService()
        {
            var installedFonts = new InstalledFontCollection();
            Families = installedFonts.Families;
        }

        public bool IsFontAvailable(string family)
            => Families.Any(f => f.Name == family);

        public FontFamily GetFontFamily(string family)
            => Families.FirstOrDefault(f => f.Name == family,
                defaultValue: Families.First(f=>f.Name==DefaultFont));

        public IEnumerable<FontFamily> FontFamilies =>
            Families.ToArray();

        public IEnumerable<string> FontNames =>
            Families.Select(f => f.Name);

    }
}
