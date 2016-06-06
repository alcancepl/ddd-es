namespace Ddd.Domain
{
    /// <summary>
    /// Represents unique id of the aggregate root.
    /// </summary>
    public interface IAggregateIdentity//: IEquatable<IAggregateIdentity>
    {
        /// <summary>
        /// The value of the unique aggregate Id.
        /// </summary>
        /// <returns></returns>
        string Value { get; }        
    }

    public static class AggregateIdentityExtensions
    {
        public static string Value(this IAggregateIdentity id)
        {
            return id.Value;
        }        
    }

    //public static class NullId<T> where T : IAggregateIdentity
    //{
    //    static NullId()
    //    {
    //        Value = default(T);
    //    }

    //    public static T Value { get; private set; }        
    //}

    //public static class NullAggregateIdentity
    //{

    //    public static bool IsNull<T>(this T id) where T : class, IAggregateIdentity
    //    {
    //        //return string.IsNullOrEmpty(id.Value);
    //        return id == NullId<T>.Value;
    //    }

    //    //    private static IAggregateIdentity NullIdentityInstance = 

    //    //    public static T Null<T>() where T : IAggregateIdentity
    //    //    {
    //    //        return 
    //    //    }
    //}
    
    //public class GuidAggregateIdentity : IAggregateIdentity
    //{

    //}
}
