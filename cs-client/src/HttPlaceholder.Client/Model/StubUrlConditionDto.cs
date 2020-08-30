/* 
 * HttPlaceholder API
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: 1.0.0
 * 
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = HttPlaceholder.Client.Client.OpenAPIDateConverter;

namespace HttPlaceholder.Client.Model
{
    /// <summary>
    /// A model for storing information about the URL condition checkers.
    /// </summary>
    [DataContract]
    public partial class StubUrlConditionDto :  IEquatable<StubUrlConditionDto>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StubUrlConditionDto" /> class.
        /// </summary>
        /// <param name="path">Gets or sets the path..</param>
        /// <param name="query">Gets or sets the query..</param>
        /// <param name="fullPath">Gets or sets the full path..</param>
        /// <param name="isHttps">Gets or sets the is HTTPS..</param>
        public StubUrlConditionDto(string path = default(string), Dictionary<string, string> query = default(Dictionary<string, string>), string fullPath = default(string), bool? isHttps = default(bool?))
        {
            this.Path = path;
            this.Query = query;
            this.FullPath = fullPath;
            this.IsHttps = isHttps;
        }
        
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>Gets or sets the path.</value>
        [DataMember(Name="path", EmitDefaultValue=true)]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>Gets or sets the query.</value>
        [DataMember(Name="query", EmitDefaultValue=true)]
        public Dictionary<string, string> Query { get; set; }

        /// <summary>
        /// Gets or sets the full path.
        /// </summary>
        /// <value>Gets or sets the full path.</value>
        [DataMember(Name="fullPath", EmitDefaultValue=true)]
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the is HTTPS.
        /// </summary>
        /// <value>Gets or sets the is HTTPS.</value>
        [DataMember(Name="isHttps", EmitDefaultValue=true)]
        public bool? IsHttps { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class StubUrlConditionDto {\n");
            sb.Append("  Path: ").Append(Path).Append("\n");
            sb.Append("  Query: ").Append(Query).Append("\n");
            sb.Append("  FullPath: ").Append(FullPath).Append("\n");
            sb.Append("  IsHttps: ").Append(IsHttps).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as StubUrlConditionDto);
        }

        /// <summary>
        /// Returns true if StubUrlConditionDto instances are equal
        /// </summary>
        /// <param name="input">Instance of StubUrlConditionDto to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StubUrlConditionDto input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Path == input.Path ||
                    (this.Path != null &&
                    this.Path.Equals(input.Path))
                ) && 
                (
                    this.Query == input.Query ||
                    this.Query != null &&
                    input.Query != null &&
                    this.Query.SequenceEqual(input.Query)
                ) && 
                (
                    this.FullPath == input.FullPath ||
                    (this.FullPath != null &&
                    this.FullPath.Equals(input.FullPath))
                ) && 
                (
                    this.IsHttps == input.IsHttps ||
                    (this.IsHttps != null &&
                    this.IsHttps.Equals(input.IsHttps))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Path != null)
                    hashCode = hashCode * 59 + this.Path.GetHashCode();
                if (this.Query != null)
                    hashCode = hashCode * 59 + this.Query.GetHashCode();
                if (this.FullPath != null)
                    hashCode = hashCode * 59 + this.FullPath.GetHashCode();
                if (this.IsHttps != null)
                    hashCode = hashCode * 59 + this.IsHttps.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}