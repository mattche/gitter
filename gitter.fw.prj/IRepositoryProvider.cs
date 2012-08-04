﻿namespace gitter.Framework
{
	using System;
	using System.Collections.Generic;

	using gitter.Framework.Configuration;

	/// <summary>Repository provider.</summary>
	public interface IRepositoryProvider : INamedObject
	{
		/// <summary>Returns GUI provider.</summary>
		/// <value>GUI provider.</value>
		IRepositoryGuiProvider GuiProvider { get; }

		/// <summary>Gets a value indicating whether this provider is loaded.</summary>
		/// <value><c>true</c> if this provider is loaded; otherwise, <c>false</c>.</value>
		bool IsLoaded { get; }

		/// <summary>Prepare to work in context of specified <paramref name="environment"/>.</summary>
		/// <param name="environment"><see cref="IWorkingEnvironment"/> to work in.</param>
		/// <param name="section">Provider configuration section.</param>
		bool LoadFor(IWorkingEnvironment environment, Section section);

		/// <summary>Save configuration to <paramref name="section"/>.</summary>
		/// <param name="section"><see cref="Section"/> for storing configuration.</param>
		void SaveTo(Section section);

		/// <summary>Checks if provider can create repository for <paramref name="workingDirectory"/>.</summary>
		/// <param name="workingDirectory">Repository working directory.</param>
		/// <returns>true, if <see cref="OpenRepository()"/> can succeed for <paramref name="workingDirectory"/>.</returns>
		bool IsValidFor(string workingDirectory);

		/// <summary>Opens repository specified by <paramref name="workingDirectory"/>.</summary>
		/// <param name="workingDirectory">Working directory of repository.</param>
		/// <returns>Opened repository.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="workingDirectory"/> == <c>null</c>.</exception>
		IRepository OpenRepository(string workingDirectory);

		/// <summary>Opens repository specified by <paramref name="workingDirectory"/>.</summary>
		/// <param name="workingDirectory">Working directory of repository.</param>
		/// <returns>Opened repository.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="workingDirectory"/> == <c>null</c>.</exception>
		IAsyncFunc<IRepository> OpenRepositoryAsync(string workingDirectory);

		/// <summary>Called after repository is successfully loaded by environment.</summary>
		/// <param name="repository">Loaded repository.</param>
		void OnRepositoryLoaded(IRepository repository);

		/// <summary>Releases all resources allocated by repository if applicable.</summary>
		/// <param name="repository">Repository to close.</param>
		/// <exception cref="ArgumentNullException"><paramref name="repository"/> == <c>null</c>.</exception>
		void CloseRepository(IRepository repository);

		/// <summary>Get list of static repository operations (that is, operations which do not require local repository).</summary>
		IEnumerable<StaticRepositoryAction> GetStaticActions();
	}
}
