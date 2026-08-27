namespace Guanomancer
{
    public static class Int32_Ext
    {
        public static int Wrap(this int value, int count)
        {
            if (Log.Fail(null, count <= 0)) return -1;

            while (value < 0) value += count;
            while (value >= count) value -= count;

            return value;
        }
    }
}