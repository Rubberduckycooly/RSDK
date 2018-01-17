#pragma once

const char VARIABLE_NAME[][0x20] =
{
	"TempValue0",						// 0x00
	"TempValue1",
	"TempValue2",
	"TempValue3",
	"TempValue4",
	"TempValue5",
	"TempValue6",
	"TempValue7",
	"CheckResult",						// 0x08
	"ArrayPos0",						// 0x09
	"ArrayPos1",
	"Global",							// 0x0B
	"Object.EntityNo",
	"Object.Type",
	"Object.PropertyValue",				// 0x0E
	"Object.XPos",
	"Object.YPos",						// 0x10
	"Object.iXPos",
	"Object.iYPos",
	"Object.State",
	"Object.Rotation",
	"Object.Scale",
	"Object.Priority",
	"Object.DrawOrder",
	"Object.Direction",
	"Object.InkEffect",
	"Object.Alpha",
	"Object.Frame",
	"Object.Animation",
	"Object.PrevAnimation",
	"Object.AnimationSpeed",
	"Object.AnimationTimer",
	"Object.Value0",					// 0x20
	"Object.Value1",
	"Object.Value2",					// 0x22
	"Object.Value3",
	"Object.Value4",
	"Object.Value5",
	"Object.Value6",
	"Object.Value7",
	"Object.OutOfBounds",
	"Player.State",
	"Player.ControlMode",
	"Player.ControlLock",
	"Player.CollisionMode",
	"Player.CollisionPlane",
	"Player.XPos",						// 0x2E
	"Player.YPos",						// 0x2F
	"Player.iXPos",						// 0x30
	"Player.iYPos",						// 0x31
	"Player.ScreenXPos",				// 0x32
	"Player.ScreenYPos",				// 0x33
	"Player.Speed",						// 0x34
	"Player.XVelocity",					// 0x35
	"Player.YVelocity",					// 0x36
	"Player.Gravity",					// 0x37
	"Player.Angle",
	"Player.Skidding",
	"Player.Pushing",
	"Player.TrackScroll",
	"Player.Up",						// 0x3C
	"Player.Down",
	"Player.Left",
	"Player.Right",
	"Player.JumpPress",					// 0x40
	"Player.JumpHold",
	"Player.FollowPlayer1",
	"Player.LookPos",
	"Player.Water",
	"Player.TopSpeed",
	"Player.Acceleration",				// 0x46
	"Player.Deceleration",
	"Player.AirAcceleration",
	"Player.AirDeceleration",
	"Player.GravityStrength",
	"Player.JumpStrength",
	"Player.JumpCap",
	"Player.RollingAcceleration",
	"Player.RollingDeceleration",
	"Player.EntityNo",
	"Player.CollisionLeft",				// 0x50
	"Player.CollisionTop",
	"Player.CollisionRight",
	"Player.CollisionBottom",
	"Player.Flailing",
	"Player.Timer",
	"Player.TileCollisions",
	"Player.ObjectInteraction",
	"Player.Visible",
	"Player.Rotation",
	"Player.Scale",
	"Player.Priority",
	"Player.DrawOrder",					// 0x5C
	"Player.Direction",
	"Player.InkEffect",
	"Player.Alpha",
	"Player.Frame",						// 0x60
	"Player.Animation",
	"Player.PrevAnimation",
	"Player.AnimationSpeed",
	"Player.AnimationTimer",
	"Player.Value0",
	"Player.Value1",
	"Player.Value2",
	"Player.Value3",
	"Player.Value4",
	"Player.Value5",
	"Player.Value6",
	"Player.Value7",
	"Player.Value8",
	"Player.Value9",
	"Player.Value10",
	"Player.Value11",
	"Player.Value12",
	"Player.Value13",
	"Player.Value14",
	"Player.Value15",
	"Player.OutOfBounds",
	"Stage.State",
	"Stage.ActiveList",
	"Stage.ListPos",					// 0x78
	"Stage.TimeEnabled",
	"Stage.MilliSeconds",				// 0x7A
	"Stage.Seconds",
	"Stage.Minutes",					// 0x7C
	"Stage.ActNo",
	"Stage.PauseEnabled",				// 0x7E
	"Stage.ListSize",
	"Stage.NewXBoundary1",				// 0x80
	"Stage.NewXBoundary2",
	"Stage.NewYBoundary1",				// 0x82
	"Stage.NewYBoundary2",
	"Stage.XBoundary1",					// 0x84
	"Stage.XBoundary2",
	"Stage.YBoundary1",					// 0x86
	"Stage.YBoundary2",
	"Stage.DeformationData0",			// 0x88
	"Stage.DeformationData1",
	"Stage.DeformationData2",
	"Stage.DeformationData3",
	"Stage.WaterLevel",					// 0x8C
	"Stage.ActiveLayer",
	"Stage.MidPoint",					// 0x8E
	"Stage.PlayerListPos",				// 0x8F
	"Stage.ActivePlayer",				// 0x90
	"Screen.CameraEnabled",
	"Screen.CameraTarget",
	"Screen.CameraStyle",				// 0x93
	"Screen.DrawListSize",
	"Screen.CenterX",
	"Screen.CenterY",					// 0x96
	"Screen.XSize",
	"Screen.YSize",						// 0x98
	"Screen.XOffset",
	"Screen.YOffset",
	"Screen.ShakeX",					// 0x9B
	"Screen.ShakeY",					// 0x9C
	"Screen.AdjustCameraY",				// 0x9D
	"TouchScreen.Down",
	"TouchScreen.XPos",
	"TouchScreen.YPos",					// 0xA0
	"Music.Volume",						// 0xA1
	"Music.CurrentTrack",				// 0xA2
	"KeyDown.Up",
	"KeyDown.Down",
	"KeyDown.Left",
	"KeyDown.Right",
	"KeyDown.ButtonA",
	"KeyDown.ButtonB",					// 0xA8
	"KeyDown.ButtonC",
	"KeyDown.Start",
	"KeyPress.Up",
	"KeyPress.Down",
	"KeyPress.Left",
	"KeyPress.Right",
	"KeyPress.ButtonA",
	"KeyPress.ButtonB",					// 0xB0
	"KeyPress.ButtonC",
	"KeyPress.Start",
	"Menu1.Selection",
	"Menu2.Selection",
	"TileLayer.XSize",
	"TileLayer.YSize",
	"TileLayer.Type",
	"TileLayer.Angle",					// 0xB8
	"TileLayer.XPos",
	"TileLayer.YPos",
	"TileLayer.ZPos",
	"TileLayer.ParallaxFactor",
	"TileLayer.ScrollSpeed",
	"TileLayer.ScrollPos",
	"TileLayer.DeformationOffset",
	"TileLayer.DeformationOffsetW",		// 0xC0
	"HParallax.ParallaxFactor",
	"HParallax.ScrollSpeed",
	"HParallax.ScrollPos",
	"VParallax.ParallaxFactor",
	"VParallax.ScrollSpeed",
	"VParallax.ScrollPos",
	"3DScene.NoVertices",
	"3DScene.NoFaces",					// 0xC8
	"VertexBuffer.x",
	"VertexBuffer.y",
	"VertexBuffer.z",
	"VertexBuffer.u",
	"VertexBuffer.v",
	"FaceBuffer.a",
	"FaceBuffer.b",
	"FaceBuffer.c",						// 0xD0
	"FaceBuffer.d",
	"FaceBuffer.Flag",
	"FaceBuffer.Color",
	"3DScene.ProjectionX",
	"3DScene.ProjectionY",				// 0xD5
	"Engine.State",						// 0xD6
	"Stage.DebugMode",					// 0xD7
	"Engine.Message",					// 0xD8
	"SaveRAM",							// 0xD9
	"Engine.Language",					// 0xDA
	"Object.SpriteSheet",				// 0xDB
	"Engine.OnlineActive",				// 0xDC
	"Engine.FrameSkipTimer",			// 0xDD
	"Engine.FrameSkipSetting",			// 0xDE
	"Engine.SFXVolume",					// 0xDF
	"Engine.BGMVolume",					// 0xE0
	"Engine.PlatformID",				// 0xE1
	"Engine.TrialMode",					// 0xE2
	"KeyPress.AnyStart",				// 0xE3
	"Engine.Haptics",					// 0xE4 ???
};