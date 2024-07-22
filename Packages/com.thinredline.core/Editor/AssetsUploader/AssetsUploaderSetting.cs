using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Editor
{
    [CreateAssetMenu(fileName = "AssetsUploaderSetting", menuName = "ThinRedLine/Assets Uploader Setting")]
    public class AssetsUploaderSetting : ScriptableObject
    {

        public List<AssetsUploader> Uploaders = new List<AssetsUploader>();
    }
}

