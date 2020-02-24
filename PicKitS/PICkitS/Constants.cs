using System;

namespace PICkitS
{
	public class Constants
	{
		public const uint PACKET_SIZE = 65u;

		public const int START_OF_STATUS_BLOCK = 32;

		public const int CB_START_INDEX = 7;

		public const int CBUF_START_INDEX = 53;

		public const byte CBUF1_WRITE = 1;

		public const uint MAX_NUM_BYTES_IN_CBUF1 = 255u;

		public const uint MAX_ARRAY_SIZE = 20480u;

		public const double FOSC = 20.0;

		public const int SCRIPT_COMPLETE_MARKER = 119;

		public const ushort LIN_PRODUCT_ID = 2564;

		public const byte BIT_MASK_0 = 1;

		public const byte BIT_MASK_1 = 2;

		public const byte BIT_MASK_2 = 4;

		public const byte BIT_MASK_3 = 8;

		public const byte BIT_MASK_4 = 16;

		public const byte BIT_MASK_5 = 32;

		public const byte BIT_MASK_6 = 64;

		public const byte BIT_MASK_7 = 128;

		public static byte[] STATUS_PACKET_DATA;
	}
}
