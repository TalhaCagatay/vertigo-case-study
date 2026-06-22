namespace com.core
{
    public static class InitOrder
    {
        private static int _number = 0;

        public static int Number
        {
            get
            {
                _number++;
                return _number;
            }
        }
    }
}