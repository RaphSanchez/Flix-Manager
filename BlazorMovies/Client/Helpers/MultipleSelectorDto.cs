namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// Represents an object that can be consumed by the
    /// MultipleSelector component. Any ancestor (consumer)
    /// of the MultipleSelector component should map its
    /// collection of items to a collection of this type.
    /// </summary>
    /// <remarks>
    /// Mapping data with Data Transfer Objects allows more
    /// control over the data that you want exposed to the
    /// user. 
    /// </remarks>
    public struct MultipleSelectorDto
    {
        public MultipleSelectorDto(int key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Primary Key essential for proper identification
        /// of each item in the original and the resulting
        /// collection. 
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// The Value to render as option to the user. 
        /// </summary>
        public string Value { get; set; }
    }
}

