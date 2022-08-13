using System.IO;
using System.Runtime.Serialization.Json;

namespace HTML.Wood.Parse.Extensions
{
    internal static class StreamExpression
    {
        public static T Deserialize<T>(this Stream deserialize)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var deserializedObj = (T)serializer.ReadObject(deserialize);
            deserialize.Close();
            return deserializedObj;
        }
    }
}
