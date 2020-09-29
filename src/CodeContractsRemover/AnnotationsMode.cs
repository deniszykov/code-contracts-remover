namespace CodeContractsRemover
{
	public enum AnnotationsMode
	{
		/// <summary> Don't add annotations </summary>
		None,

		/// <summary> Include only for work in Visual Studio. When project is compiled, annotations would be removed.
		/// When dll is referenced, Rider/Re# wouldn't show hints.
		/// This mode is recommended for applications</summary>
		Add,

		/// <summary> Annotations will be included into binaries.
		/// When dll is referenced, Rider/Re# would show hints.
		/// This mode is recommended for packages</summary>
		IncludeIntoBinaries
	}
}
