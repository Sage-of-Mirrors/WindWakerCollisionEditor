using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindWakerCollisionEditor
{
    public class PropertyUndoRedo : IUndoRedoCommand
    {
        /// <summary>
        /// Represents the property that this command will perform an action on
        /// </summary>
        PropertyType property;

        /// <summary>
        /// The value that the property had originally
        /// </summary>
        object oldValue;
        /// <summary>
        /// The new value of the property
        /// </summary>
        object newValue;

        /// <summary>
        /// A list of triangles to perform an action on
        /// </summary>
        List<Triangle> triList;

        /// <summary>
        /// Instantiates a new PropertyUndoRedo command.
        /// </summary>
        /// <param name="prop">Property this command works with</param>
        /// <param name="tria">List of triangles to perform an action on</param>
        /// <param name="oldVal">Previous value that the property had</param>
        /// <param name="newVal">New value that the property has</param>
        public PropertyUndoRedo(PropertyType prop, List<Triangle> tria, object oldVal, object newVal)
        {
            property = prop;
            triList = tria;
            oldValue = oldVal;
            newValue = newVal;
        }

        public void Execute()
        {
            foreach (Triangle tri in triList)
            {
                switch (property)
                {
                    case PropertyType.AttributeCode:
                        tri.AttributeCode = (AttributeCode)newValue;
                        break;
                    case PropertyType.CamBehavior:
                        tri.CameraBehavior = (int)newValue;
                        break;
                    case PropertyType.CameraID:
                        tri.CameraID = (int)newValue;
                        break;
                    case PropertyType.CamMovBG:
                        tri.CamMoveBG = (int)newValue;
                        break;
                    case PropertyType.ExitID:
                        tri.ExitIndex = (int)newValue;
                        break;
                    case PropertyType.GroundCode:
                        tri.GroundCode = (GroundCode)newValue;
                        break;
                    case PropertyType.LinkNo:
                        tri.LinkNumber = (int)newValue;
                        break;
                    case PropertyType.PolyColor:
                        tri.PolyColor = (int)newValue;
                        break;
                    case PropertyType.RoomCamID:
                        tri.RoomCamID = (int)newValue;
                        break;
                    case PropertyType.RoomPathID:
                        tri.RoomPathID = (int)newValue;
                        break;
                    case PropertyType.RoomPathPointID:
                        tri.RoomPathPointNo = (int)newValue;
                        break;
                    case PropertyType.SoundID:
                        tri.SoundID = (SoundID)newValue;
                        break;
                    case PropertyType.SpecialCode:
                        tri.SpecialCode = (SpecialCode)newValue;
                        break;
                    case PropertyType.WallCode:
                        tri.WallCode = (WallCode)newValue;
                        break;
                }
            }
        }

        public void UnExecute()
        {
            foreach (Triangle tri in triList)
            {
                switch (property)
                {
                    case PropertyType.AttributeCode:
                        tri.AttributeCode = (AttributeCode)oldValue;
                        break;
                    case PropertyType.CamBehavior:
                        tri.CameraBehavior = (int)oldValue;
                        break;
                    case PropertyType.CameraID:
                        tri.CameraID = (int)oldValue;
                        break;
                    case PropertyType.CamMovBG:
                        tri.CamMoveBG = (int)oldValue;
                        break;
                    case PropertyType.ExitID:
                        tri.ExitIndex = (int)oldValue;
                        break;
                    case PropertyType.GroundCode:
                        tri.GroundCode = (GroundCode)oldValue;
                        break;
                    case PropertyType.LinkNo:
                        tri.LinkNumber = (int)oldValue;
                        break;
                    case PropertyType.PolyColor:
                        tri.PolyColor = (int)oldValue;
                        break;
                    case PropertyType.RoomCamID:
                        tri.RoomCamID = (int)oldValue;
                        break;
                    case PropertyType.RoomPathID:
                        tri.RoomPathID = (int)oldValue;
                        break;
                    case PropertyType.RoomPathPointID:
                        tri.RoomPathPointNo = (int)oldValue;
                        break;
                    case PropertyType.SoundID:
                        tri.SoundID = (SoundID)oldValue;
                        break;
                    case PropertyType.SpecialCode:
                        tri.SpecialCode = (SpecialCode)oldValue;
                        break;
                    case PropertyType.WallCode:
                        tri.WallCode = (WallCode)oldValue;
                        break;
                }
            }
        }

        public bool Compare(IUndoRedoCommand command)
        {
            return false;
        }
    }
}
