namespace Dallal_Backend_v2.Helpers;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DoNotIncludeInSubmissionAttribute : Attribute { }
