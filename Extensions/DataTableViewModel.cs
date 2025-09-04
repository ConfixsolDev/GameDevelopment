namespace TechWebSol.Extensions
{
    public class JQDTParams
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public JQDTColumnSearch search { get; set; } = new();
        public List<JQDTColumnOrder> order { get; set; } = new();
        public List<JQDTColumn> columns { get; set; } = new();
        public List<FilterColumns> filters { get; set; } = new();
    }

    public class FilterColumns
    {
        public string columnName { get; set; }
        public string columnValue { get; set; }
    }

    public enum JQDTColumnOrderDirection
    {
        asc, desc
    }

    public class JQDTColumnOrder
    {
        public int column { get; set; }
        public JQDTColumnOrderDirection dir { get; set; }
    }

    public class JQDTColumnSearch
    {
        public string value { get; set; }
        public bool regex { get; set; }
    }

    public class JQDTColumn
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public JQDTColumnSearch search { get; set; }
    }
}
