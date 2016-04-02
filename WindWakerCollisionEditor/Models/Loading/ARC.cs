using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WArchiveTools;
using GameFormatReader.Common;
using System.IO;
using WArchiveTools.FileSystem;

namespace WindWakerCollisionEditor
{
    class ARC : IModelSource
    {
        ObservableCollection<Category> m_catList;

        public ARC()
        {
            m_catList = new ObservableCollection<Category>();
        }

        public IEnumerable<Category> Load(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                VirtualFilesystemDirectory rootDir = ArchiveUtilities.LoadArchive(fileName);

                foreach (VirtualFilesystemNode node in rootDir.Children)
                {
                    if (node.Name.Contains("dzb"))
                    {
                        return GetDzbData((VirtualFilesystemDirectory)node);
                    }
                }
            }

            // If this is triggered, the archive the user opened wasn't a room archive and didn't have any DZBs in it
            return null;
        }

        private IEnumerable<Category> GetDzbData(VirtualFilesystemDirectory dir)
        {
            DZB loader = new DZB();

            VirtualFilesystemFile dzbFile;

            // We'll search for room.dzb and load that if we can find it
            foreach (VirtualFilesystemNode node in dir.Children)
            {
                if ((node.Name == "room") && (node.Type == NodeType.File))
                {
                    dzbFile = node as VirtualFilesystemFile;

                    EndianBinaryReader reader = new EndianBinaryReader(dzbFile.File.GetData(), Endian.Big);

                    return loader.Load(reader);
                }
            }

            // If we get here, this isn't a room arc, so let's just load the first DZB file
            if (dir.Children[0].Type == NodeType.File)
            {
                dzbFile = dir.Children[0] as VirtualFilesystemFile;

                EndianBinaryReader reader = new EndianBinaryReader(dzbFile.File.GetData(), Endian.Big);

                return loader.Load(reader);
            }

            // If this happens, my tool has serious issues
            return null;
        }
    }
}
