using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ProcessOCRImageResponseModel
    {
        public List<OCRItem> brand_names { get; set; }
        public List<OCRItem> model_numbers { get; set; }
        public List<OCRItem> serial_numbers { get; set; }
    }
    public class OCRItem
    {
        public string value { get; set; }
        public List<int[]> box { get; set; }
        public double confidence { get; set; }
    }
    public class Box
    {
        public List<int[]> box { get; set; }
    }
}
