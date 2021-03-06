#region Copyright Notice
/*
 * gitter - VCS repository management tool
 * Copyright (C) 2013  Popovskiy Maxim Vladimirovitch <amgine.gitter@gmail.com>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

namespace gitter.Git.Gui.Controls
{
	using System;
	using System.ComponentModel;
	using System.Windows.Forms;

	using Resources = gitter.Git.Gui.Properties.Resources;

	[ToolboxItem(false)]
	public sealed class ConfigParameterMenu : ContextMenuStrip
	{
		private readonly ConfigParameterListItem _listItem;
		private readonly ConfigParameter _parameter;

		public ConfigParameterMenu(ConfigParameterListItem listItem)
		{
			Verify.Argument.IsNotNull(listItem, "listItem");
			Verify.Argument.IsValidGitObject(listItem.DataContext, "parameter");

			_listItem = listItem;
			_parameter = listItem.DataContext;

			Items.Add(new ToolStripMenuItem(Resources.StrEditValue, CachedResources.Bitmaps["ImgConfigEdit"], (s, e) => listItem.StartValueEditor()));
			Items.Add(GuiItemFactory.GetUnsetParameterItem<ToolStripMenuItem>(_parameter));
			Items.Add(new ToolStripSeparator());
			Items.Add(new ToolStripMenuItem(Resources.StrCopyToClipboard, null,
				GuiItemFactory.GetCopyToClipboardItem<ToolStripMenuItem>(Resources.StrName, _parameter.Name),
				GuiItemFactory.GetCopyToClipboardItem<ToolStripMenuItem>(Resources.StrValue, _parameter.Value)));
		}

		public ConfigParameterMenu(ConfigParameter parameter)
		{
			Verify.Argument.IsValidGitObject(parameter, "parameter");

			_parameter = parameter;
			Items.Add(GuiItemFactory.GetUnsetParameterItem<ToolStripMenuItem>(parameter));
			Items.Add(new ToolStripSeparator());
			Items.Add(new ToolStripMenuItem(Resources.StrCopyToClipboard, null,
				GuiItemFactory.GetCopyToClipboardItem<ToolStripMenuItem>(Resources.StrName, _parameter.Name),
				GuiItemFactory.GetCopyToClipboardItem<ToolStripMenuItem>(Resources.StrValue, _parameter.Value)));
		}

		public ConfigParameter ConfigParameter
		{
			get { return _parameter; }
		}
	}
}
