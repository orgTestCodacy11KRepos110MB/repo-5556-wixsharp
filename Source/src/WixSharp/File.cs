#region Licence...

/*
The MIT License (MIT)

Copyright (c) 2014 Oleg Shilo

Permission is hereby granted,
free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

#endregion Licence...

using System;
using System.Linq;
using IO = System.IO;

namespace WixSharp
{
    /// <summary>
    /// Defines file to be installed.
    /// </summary>
    ///
    ///<example>The following is an example of installing <c>MyApp.exe</c> file.
    ///<code>
    /// var project =
    ///     new Project("My Product",
    ///
    ///         new Dir(@"%ProgramFiles%\My Company\My Product",
    ///
    ///             new File(binaries, @"AppFiles\MyApp.exe",
    ///                 new WixSharp.Shortcut("MyApp", @"%ProgramMenu%\My Company\My Product"),
    ///                 new WixSharp.Shortcut("MyApp", @"%Desktop%")),
    ///
    ///         ...
    ///
    /// Compiler.BuildMsi(project);
    /// </code>
    /// </example>
    public class File : WixEntity
    {
        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the <see cref="File"/>.
        /// <para>This property is designed to produce a friendlier string representation of the <see cref="File"/>
        /// for debugging purposes.</para>
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the <see cref="File"/>.
        /// </returns>
        public new string ToString()
        {
            return IO.Path.GetFileName(Name) + "; " + Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="File"/> class.
        /// </summary>
        public File() { }

        /// <summary>
        /// Creates instance of the <see cref="File"></see> class with properties initialized with specified parameters.
        /// </summary>
        /// <param name="feature"><see cref="Feature"></see> the file should be included in.</param>
        /// <param name="sourcePath">Relative path to the file to be taken for building the MSI.</param>
        /// <param name="items">Optional parameters defining additional members (e.g. <see cref="FileShortcut"/> shortcuts
        /// to the file to be created during the installation).</param>
        public File(Feature feature, string sourcePath, params WixEntity[] items)
        {
            Name = sourcePath;
            Feature = feature;
            AddItems(items);
        }

        /// <summary>
        /// Creates instance of the <see cref="File"></see> class with properties initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="File"/> instance.</param>
        /// <param name="feature"><see cref="Feature"></see> the file should be included in.</param>
        /// <param name="sourcePath">Relative path to the file to be taken for building the MSI.</param>
        /// <param name="items">Optional parameters defining additional members (e.g. <see cref="FileShortcut"/> shortcuts
        /// to the file to be created during the installation).</param>
        public File(Id id, Feature feature, string sourcePath, params WixEntity[] items)
        {
            Id = id.Value;
            Name = sourcePath;
            Feature = feature;
            AddItems(items);
        }

        /// <summary>
        /// Creates instance of the <see cref="File"></see> class with properties initialized with specified parameters.
        /// </summary>
        /// <param name="sourcePath">Relative path to the file to be taken for building the MSI.</param>
        /// <param name="items">Optional parameters defining additional members (e.g. <see cref="FileShortcut"/> shortcuts
        /// to the file to be created during the installation).</param>
        public File(string sourcePath, params WixEntity[] items)
        {
            Name = sourcePath;
            AddItems(items);
        }

        /// <summary>
        /// Creates instance of the <see cref="File"></see> class with properties initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="File"/> instance.</param>
        /// <param name="sourcePath">Relative path to the file to be taken for building the MSI.</param>
        /// <param name="items">Optional parameters defining additional members (e.g. <see cref="FileShortcut"/> shortcuts
        /// to the file to be created during the installation).</param>
        public File(Id id, string sourcePath, params WixEntity[] items)
        {
            Id = id.Value;
            Name = sourcePath;
            AddItems(items);
        }

        void AddItems(WixEntity[] items)
        {
            Shortcuts = items.OfType<FileShortcut>().ToArray();
            Associations = items.OfType<FileAssociation>().ToArray();
            IISVirtualDirs = items.OfType<IISVirtualDir>().ToArray();
            ServiceInstallers = items.OfType<ServiceInstaller>().Cast<IGenericEntity>().ToArray();
            Permissions = items.OfType<FilePermission>().ToArray();
            AppIds = items.OfType<AppId>().ToArray();
            GenericItems = items.OfType<IGenericEntity>().Where(val => val.GetType() != typeof(ServiceInstaller)).ToArray();

            FirewallExceptions = items.OfType<FirewallException>().ToArray();

            var firstUnExpectedItem = items.Except(Shortcuts)
                                           .Except(Associations)
                                           .Except(IISVirtualDirs)
                                           .Except(Permissions)
                                           .Except(FirewallExceptions)
                                           .Except(AppIds)
                                           .Except(GenericItems.Cast<WixEntity>())
                                           .Where(x => !(x is IGenericEntity) || !ServiceInstallers.Contains(x as IGenericEntity))
                                           .ToArray();

            if (firstUnExpectedItem.Any())
                throw new ApplicationException("{0} is unexpected. Only {2}, {3}, {4}, {5}, {6}, {7}, and {8} items can be added to {1}".FormatWith(
                                               firstUnExpectedItem.First().GetType(),
                                               this.GetType(),
                                               typeof(FileShortcut),
                                               typeof(FileAssociation),
                                               typeof(ServiceInstaller),
                                               typeof(FilePermission),
                                               typeof(FirewallException),
                                               typeof(AppId),
                                               typeof(IGenericEntity)));
        }

        /// <summary>
        /// Collection of the <see cref="FileAssociation"/>s associated with the file.
        /// </summary>
        public FileAssociation[] Associations = new FileAssociation[0];

        /// <summary>
        /// The service installer associated with the file.
        ///  Set this field to the properly initialized instance of <see cref="ServiceInstaller"/> if the file is a windows service module.
        /// </summary>
        public IGenericEntity ServiceInstaller
        {
            get => ServiceInstallers?.FirstOrDefault();
            set
            {
                if (value != null)
                {
                    ServiceInstallers = new[]
                    {
                        value
                    };
                }
            }
        }

        /// <summary>
        /// The service installers associated with the file.
        ///  Set this field to the collection of the properly initialized instances of <see cref="ServiceInstaller"/> if the file is a windows service module.
        ///  <para>This member is designed to allow installing multiple services based on a single file. For the trivial cases of "single file - single service"
        ///  use <see cref="WixSharp.File.ServiceInstaller"/></para>
        /// </summary>>
        public IGenericEntity[] ServiceInstallers = null;

        /// <summary>
        /// Collection of the contained <see cref="IISVirtualDir"/>s.
        /// </summary>
        public IISVirtualDir[] IISVirtualDirs = new IISVirtualDir[0];

        /// <summary>
        /// Collection of the <see cref="Shortcut"/>s associated with the file.
        /// </summary>
        public FileShortcut[] Shortcuts = new FileShortcut[0];

        /// <summary>
        /// Defines the installation <see cref="Condition"/>, which is to be checked during the installation to
        /// determine if the file should be installed on the target system.
        /// </summary>
        public Condition Condition;

        /// <summary>
        /// Controls if an existing file should be overwritten during the installation.
        /// By default MSI runtime does not install the file if it already exists at the
        /// deployment destination on the target system. This field allows changing
        /// this behaviour and ensuring that a file installed always even when existed
        /// prior the installation.
        /// <para>If this field is set to <c>true</c> the WixSharp injects the following
        /// element into the file's parent component.
        /// <pre>&lt;RemoveFile Id="Remove_Filetxt" Name="File.txt" On="install" /&gt;</pre>
        ///
        /// </para>
        /// </summary>
        public bool OverwriteOnInstall = false;

        /// <summary>
        /// Collection of <see cref="T:WixSharp.FilePermission" /> to be applied to the file.
        /// </summary>
        public FilePermission[] Permissions = new FilePermission[0];

        /// <summary>
        /// Collection of <see cref="T:WixSharp.FirewallException" /> to be applied to the file.
        /// </summary>
        public FirewallException[] FirewallExceptions
        {
            get => GenericItems.OfType<FirewallException>().ToArray();
            set => GenericItems = GenericItems.Concat(value).Distinct().ToArray();
        }

        /// <summary>
        /// DCOM AppIds associated with this file.
        /// </summary>
        public AppId[] AppIds = new AppId[0];

        /// <summary>
        /// Collection of <see cref="T:WixSharp.IGenericEntity" /> to be applied to the file.
        /// </summary>
        public IGenericEntity[] GenericItems = new IGenericEntity[0];

        /// <summary>
        /// Gets or sets the NeverOverwrite attribute of the associated WiX component.
        /// <para>If this attribute is set to 'true', the installer does not install or reinstall the component
        ///  if a key path file for the component already exists. </para>
        /// </summary>
        /// <value>
        /// The never overwrite.
        /// </value>
        public bool? NeverOverwrite
        {
            get => attributesBag.Get("Component:NeverOverwrite") == "yes";

            set => attributesBag.Set("Component:NeverOverwrite", value.ToNullOrYesNo());
        }

        /// <summary>
        /// The flag that indicates if <c>CloseApplication</c> needs to be added for the specified file.
        /// <example>
        /// <code>
        /// new Project("MyProduct",
        ///     new Dir(@"%ProgramFiles%\My Company\My Product",
        ///         new File(@"..\Install Files\Files\Bin\MyApp.exe") { AddCloseAction = true }),
        ///     ...
        ///
        /// // the code above is equivalent of code below
        ///
        /// new Project("MyProduct",
        ///     new Dir(@"%ProgramFiles%\My Company\My Product",
        ///         new File(@"..\Install Files\Files\Bin\MyApp.exe"),
        ///     new CloseApplication("MyApp.exe", true, false)),
        ///     ...
        /// </code>
        /// </example>
        /// </summary>
        public bool? AddCloseAction;

        /// <summary>
        /// Gets or sets the custom name of the target file. By default the name is
        /// the same name as the source file. IE file <c>c:\files\app.exe</c> will be installed
        /// as <c>app.exe</c>.
        /// </summary>
        /// <value>
        /// The name of the target file.
        /// </value>
        public string TargetFileName
        {
            get => attributesBag.Get("Name");

            set => attributesBag.Set("Name", value);
        }
    }
}