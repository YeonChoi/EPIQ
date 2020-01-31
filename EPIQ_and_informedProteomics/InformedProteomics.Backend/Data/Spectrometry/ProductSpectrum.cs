using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace InformedProteomics.Backend.Data.Spectrometry
{
    public class ProductSpectrum: Spectrum
    {
        public ProductSpectrum(double[] mzArr, double[] intensityArr, int scanNum) : base(mzArr, intensityArr, scanNum)
        {
        }

        public ProductSpectrum(ICollection<Peak> peaks, int scanNum): base(peaks, scanNum)
        {
        }

        public ActivationMethod ActivationMethod { get; set; }
        public IsolationWindow IsolationWindow { get; set; }

        public void SetMsLevel(int msLevel)
        {
            MsLevel = msLevel;
        }

        public string ToMgfString()
        {
            return ToMgfString(IsolationWindow.Charge, ScanNum, NativeId);
        }

        public string ToMgfString(int? charge, int scanNumber, string title)
        {
            return ToMgfString(charge, IsolationWindow.IsolationWindowTargetMz, scanNumber, title);
        }

        public string ToMgfString(int? charge, double targetMz, int scanNumber, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BEGIN IONS");
            sb.Append("TITLE=");
            sb.AppendLine(title);
            sb.Append("ACTIVATION=");
            sb.AppendLine(ActivationMethod.ToString());
            sb.Append("PEPMASS=");
            sb.AppendLine(targetMz.ToString());

            if (scanNumber > 0)
            {
                sb.Append("SCANS=");
                sb.AppendLine(scanNumber.ToString());
            }            
            
            sb.Append("CHARGE=");
            sb.AppendLine((charge ?? 2).ToString());            
            
            foreach(var p in this.Peaks){
                if (p.Intensity > 0)
                {
                    sb.Append(p.Mz.ToString());
                    sb.Append('\t');
                    sb.AppendLine(p.Intensity.ToString());
                }
            }
            sb.AppendLine("END IONS");
            return sb.ToString();
        }
    }
}
