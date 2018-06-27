namespace nsjsdotnet
{
    public enum NSJSDataType : uint
    {
        kUndefined = 0x000000,
        kString = 0x000001,
        kInt32 = 0x000002,
        kDouble = 0x000004,
        kBoolean = 0x000008,
        kNull = 0x000010,
        kDateTime = 0x000020,
        kFunction = 0x000040,
        kObject = 0x000080,
        kArray = 0x000100,
        kUInt32 = 0x000200,
        kInt32Array = 0x000400,
        kUInt32Array = 0x000800,
        kInt8Array = 0x001000,
        kUInt8Array = 0x002000,
        kInt16Array = 0x004000,
        kUInt16Array = 0x008000,
        kFloat32Array = 0x010000,
        kFloat64Array = 0x020000,
        kInt64 = 0x040000,
    }
}
