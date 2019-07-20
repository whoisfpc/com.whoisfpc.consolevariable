namespace ConsoleVariable
{
    public class CCWrapper
    {
        public enum WrappedType
        {
            CVar,
            CCmd
        }
        public string Name
        {
            get
            {
                if (type == WrappedType.CVar)
                {
                    return cvar.Name;
                }
                else
                {
                    return cmd.Name;
                }
            }
        }
        public WrappedType type;
        public CVariable cvar;
        public CCommand cmd;
    }
}
