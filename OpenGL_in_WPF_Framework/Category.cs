using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CollisionEditor
{
    public class Category : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged overhead

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private static int m_nameAddInt;

        public string Name
        {
            get { return m_name; }
            set
            {
                if (value != m_name)
                {
                    m_name = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private string m_name;

        public BindingList<Group> Groups
        {
            get { return m_groups; }
            set
            {
                if (value != m_groups)
                {
                    m_groups = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private BindingList<Group> m_groups;

        public int InitialIndex;

        public Category()
        {
            m_name = "NewCategory" + m_nameAddInt++;

            m_groups = new BindingList<Group>();
        }

        public Category(EndianBinaryReader stream)
        {
            m_groups = new BindingList<Group>();

            int streamStart = (int)stream.BaseStream.Position;

            stream.BaseStream.Position = stream.ReadInt32();

            char[] tempChars = Encoding.ASCII.GetChars(stream.ReadBytesUntil(0));

            m_name = new string(tempChars);

            stream.BaseStream.Position = streamStart + 0x34;
        }
    }
}
