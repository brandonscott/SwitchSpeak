﻿/* DynamicKeySettings.cs
 *
 * Copyright © 2013 by Adam Hellberg and Brandon Scott.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Disclaimer: SharpBlade is in no way affiliated
 * with Razer and/or any of its employees and/or licensors.
 * Adam Hellberg does not take responsibility for any harm caused, direct
 * or indirect, to any Razer peripherals via the use of SharpBlade.
 * 
 * "Razer" is a trademark of Razer USA Ltd.
 */

using Sharparam.SharpBlade.Native;
using Sharparam.SharpBlade.Razer.Events;

namespace Sharparam.SharpBlade
{
    /// <summary>
    /// Settings for a dynamic key.
    /// </summary>
    public struct DynamicKeySettings
    {
        /// <summary>
        /// The key type these settings are describing.
        /// </summary>
        public readonly RazerAPI.DynamicKeyType Key;

        /// <summary>
        /// Image used for the UP state.
        /// </summary>
        public readonly string UpImage;

        /// <summary>
        /// Image used for the DOWN state.
        /// </summary>
        public readonly string DownImage;

        /// <summary>
        /// Handler function called when this key is pressed.
        /// </summary>
        public readonly DynamicKeyPressedEventHandler Handler;

        /// <summary>
        /// Creates a new <see cref="DynamicKeySettings" /> structure.
        /// </summary>
        /// <param name="key">The key for which these settings are.</param>
        /// <param name="image">Image to use for both UP and DOWN states.</param>
        /// <param name="handler">Handler called when this key is pressed.</param>
        public DynamicKeySettings(RazerAPI.DynamicKeyType key, string image, DynamicKeyPressedEventHandler handler)
            : this(key, image, image, handler)
        {
            
        }

        /// <summary>
        /// Creates a new <see cref="DynamicKeySettings" /> structure.
        /// </summary>
        /// <param name="key">The key for which these settings are.</param>
        /// <param name="upImage">Image to use for the UP state.</param>
        /// <param name="downImage">Image to use for the DOWN state.</param>
        /// <param name="handler">Handler called when this key is pressed.</param>
        public DynamicKeySettings(RazerAPI.DynamicKeyType key, string upImage, string downImage,
                                  DynamicKeyPressedEventHandler handler)
        {
            Key = key;
            UpImage = upImage;
            DownImage = downImage;
            Handler = handler;
        }
    }
}
