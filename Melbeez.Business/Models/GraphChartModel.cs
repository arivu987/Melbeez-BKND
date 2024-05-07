using System.Collections.Generic;

namespace Melbeez.Business.Models
{
    public class GraphChartModel
    {
        public string type { get; set; }
        public GraphDataModel data { get; set; }
        public Option options { get; set; } = new Option();
    }

    public class GraphDataModel
    {
        public List<string> labels { get; set; }
        public List<DatasetModel> datasets { get; set; }
    }

    public class DatasetModel
    {
        public string label { get; set; }
        public List<string> backgroundColor { get; set; }
        //public List<string> borderColor { get; set; }
        public List<decimal?> data { get; set; } = new List<decimal?>();
        public bool fill { get; set; } = false;
    }

    public class TicksAxesY
    {
        public TicksAxesY()
        {
            beginAtZero = true;
        }
        public bool beginAtZero { get; set; }
    }

    public class AxesY
    {
        public AxesY()
        {
            ticks = new TicksAxesY();
        }
        public TicksAxesY ticks { get; set; }
    }

    public class Point
    {
        public Point(string _x, double? _y)
        {
            x = _x;
            y = _y;
        }
        public string x { get; set; }
        public double? y { get; set; }
    }

    public class Option
    {
        public bool responsive { get; set; } = true;
        public Tooltips tooltips { get; set; } = new Tooltips() { enabled = true };
        public Scales scales { get; set; } = new Scales();
        public Layout layout { get; set; } = new Layout();
        public Plugins plugins { get; set; } = new Plugins();
        public string indexAxis { get; set; } = null;
        public bool maintainAspectRatio { get; set; } = true;
    }

    public class Legend
    {
        public Labels labels { get; set; } = new Labels();
        public string position { get; set; } = "bottom";
    }

    public class Labels
    {
        public bool usePointStyle { get; set; } = true;
        public string pointStyle { get; set; } = "circle";
        public string color { get; set; } = "#808080";
        public Fonts fonts { get; set; }
    }

    public class Tooltips
    {
        public bool enabled { get; set; }
    }

    public class Layout
    {
        public Padding padding { get; set; } = new Padding();
    }

    public class Padding
    {
        public int bottom { get; set; } = 10;
        public int left { get; set; } = 10;
        public int right { get; set; } = 10;
        public int top { get; set; } = 20;
    }

    public class Plugins
    {
        //public bool labels { get; set; } = false;
        public Legend legend { get; set; } = new Legend();
    }

    public class Datalabels
    {
        public int fontSize { get; set; } = 14;
        public string fontStyle { get; set; } = "bold";
        public string fontColor { get; set; } = "#000";
    }

    public class Scales
    {
        public Scales()
        {
            x = new xAxes();
            y = new yAxes();
        }
        public xAxes x { get; set; }
        public yAxes y { get; set; }
    }

    public class xAxes
    {
        public xAxes()
        {
            ticks = new Ticks();
        }

        public bool stacked { get; set; } = false;
        public Ticks ticks { get; set; }
    }

    public class yAxes
    {
        public yAxes()
        {
            ticks = new Ticks();
        }

        public bool stacked { get; set; } = false;
        public Ticks ticks { get; set; }
    }

    public class Ticks
    {
        public bool beginAtZero { get; set; } = true;
        public string color { get; set; } = "#808080";
        public int min { get; set; } = 0;
        public int stepSize { get; set; }
        public Fonts fonts { get; set; }
    }

    public class Fonts
    {
        public int size { get; set; } = 12;
    }
}
