namespace Project.Scripts.General
{
    /// <summary>
    ///   <para>All classes that implement this interface will have the "ForceAwake" function called in the unity awake context by the ForceAwakener</para>   
    /// </summary>
    public interface IShouldForceAwake
    {
        public void ForceAwake();
    }
}
