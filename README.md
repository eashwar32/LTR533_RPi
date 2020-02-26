#linux configuration

1.LTR 533 project was same as windows
2.PicKits - USB_Read.cs - prepend zeros in read buffer to make 64bytes to 65 bytes 
     to follow windows.Need to seggregate based on os type so source code maintainance will not overhead for both platforms.
3.Added sleep mechanism in PicKits - USB_Read.cs to avoid hid read overhead temporarily. 