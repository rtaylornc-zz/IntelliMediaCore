//---------------------------------------------------------------------------------------
// Copyright 2014 North Carolina State University
//
// Center for Educational Informatics
// http://www.cei.ncsu.edu/
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistributions of source code must retain the above copyright notice, this 
//     list of conditions and the following disclaimer.
//   * Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace IntelliMedia
{
	public class GazeEventArgs : EventArgs
	{
        // Current position in pixel coordinates 
        // (origin is in lower left of screen)
		public float LeftX { get; set; }
		public float LeftY { get; set; }
		public float LeftPupilDiameter { get; set; }
		public float RightX { get; set; }
		public float RightY { get; set; }
		public float RightPupilDiameter { get; set; }
	}

    public abstract class EyeTracker
    {
		public delegate void CalibrationResultHandler(bool success, string message, Dictionary<string, object> calibrationPropertyResults);

		public event EventHandler<GazeEventArgs> GazeChanged;
		
		protected void OnGazeChanged(GazeEventArgs e)
		{
			if (GazeChanged != null)
			{
				GazeChanged(this, e);
			} 
		}

		public bool IsConnected { get; protected set; }
		public bool IsCalibrated { get; protected set; }

		public virtual void Connect() {}
		public virtual void Disconnect() {}

        // TODO rgtaylor 2015-03-18 This is really only need by SimulatedEyeTracker as a way to 
        // fake incoming data. Ideally, this could be handed by a Dispatcher that hooks into
        // the platforms 'ticking mechanism'
        public virtual void Update() {}

		public bool IsCalibrationRequired { get; protected set; }
		public virtual void Calibrate(CalibrationResultHandler callback) 
		{
			throw new NotImplementedException("Calibration is not required for " + this.GetType().Name);
		}
    }
}

