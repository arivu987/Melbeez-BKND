namespace Melbeez.Common.Models.Entities
{
    public class FieldBaseModel
    {
        public string Id { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
    }

    public class FormFieldRequestModel : FieldBaseModel
    {
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public string RequiredMessage { get; set; }
        public string Placeholder { get; set; }
        public int Sequence { get; set; }
        public Dropdowndata[]? DropdownData { get; set; }
    }
    public class Dropdowndata
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }
}
