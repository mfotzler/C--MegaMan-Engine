   M S B A M L   `   `   `  ����M  IMegaManLevelEditor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullU=clr-namespace:MegaMan.LevelEditor;assembly=MegaManLevelEditorMegaMan.LevelEditor  5   	    	WPFEditor   WPFEditor.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6F   Peditor=clr-namespace:MegaMan.LevelEditor;assembly=MegaManLevelEditor     5   	     � Title$  Mega Man Level Editor��5   	   $	��776��6'   $
��1107��64   .��5      �  � RowDefinitions 5   
   �� 5	      $
��auto �6   �� 5
      .+�5   
   o� $��0��6   .@�5      m� $��_File��6   .@�5      m� $
��_New��6    m� Command$	 New��6)    m� Icon 5      �� 5      $"��/Resources/NewDocumentHS.png��6    5      m� 5      $��_Open��6   $
 Open��6*    5      �� 5      $��/Resources/openHS.png��6    5      m� 5      $��_Recent��6   �� 5      m� 5      $��_Save��6   $
 Save��6*    5      �� 5      $��/Resources/saveHS.png��6    5      �� 5      m� 5       $��_Close��6   $ Close��6+   m� 5!      $��_Quit��6   5"      m� 5#      $��_Edit��6   .@�5$      m� $
��Undo��6   $
 Undo��6)    5%      �� 5&      $��/Resources/Undo.png��6    5(      m� 5)      $
��Redo��6   $
 Redo��6)    5*      �� 5+      $��/Resources/Redo.png��6    5-      �� 5.      m� 5/      $
��Copy��6   $
 Copy��6)    50      �� 51      $��/Resources/CopyHS.png��6    53      m� 54      $��Paste��6   $ Paste��6*    55      �� 56      $��/Resources/PasteHS.png��6    59      m� 5:      $��Paste as Brush��6    5;      �� 5<      $��/Resources/PasteBrush.png��6    5>      5?      m� 5@      $��_View��6   .@�5A      m� $��Tiles��6   m� 5B      $��Blocking��6   m� 5C      $��Entities��6   m� 5D      $
��Grid��6   m� 5E      $��Animate��6   m� 5F      $��Joins��6   5G      m� 5H      $��_Stage��6   .@�5I      m� $��	New Stage��6   m� 5J      $��
Properties��6   m� 5K      $��Stage Select...��6   5L      m� 5M      $��_Screen��6   .@�5N      m� $��Split Screen��6   m� 5O      $��Merge Screen��6   �� 5P      m� 5Q      $��Clear Screen��6   5R      m� 5S      $��_Windows��6   .@�5T      m� $��Brushes��6   m� 5U      $��Project��6   m� 5V      $��History��6   m� 5W      $��Entities��6   5X      m� 5Y      $��_Test��6   .@�5Z      m� $��Run Project��6   m� 5[      $��	Run Stage��6   �� 5\      m� 5]      $��Set Engine Path��6   5^      5_      �� 5a   
   $��1��6    � 	Resources 5b       �� 
TargetType �� Property �� Value �� 
TargetType	 ��Name
 �� Triggers P� Property P� Value �� 
TargetName��%�  '��       '�� K     '�� [    �� 5c      " ��6   .�5d      ��  � ��$ Right=�6<   5e      �� 5f      " ��6   .�5g      ��  � �� 5h      �� 5i      " ��6.   .R�5j   %   �� $	 ContentSite��66   $��Center=�5k   6   $��Center=�5l   6   $��Header��5m   6   $��30,10,30,10q�5n   6   $��18(�5o      $��	Megaman 2)�5p      
 5q   "   P� 5r   &    �@��
IsSelected . . �5s   *   �� $ ContentSite��61    � ��
 ����6j   5t   '   P� 5u   &    �@��
IsSelected .  . �5v   *   �� $ ContentSite��61    � ��
 �   �6j   5w   '   5y      5{      5|      �� 5}      " ��6   .�5~      ��  � ��$ 0,0,0,0q�68   6J   5      5�      .@�5�      �� $��Layout��5�   "   .��5�      7  1System.Windows.Forms.Integration.WindowsFormsHost  $
��Left=�6#   $	��Top=�6>      Child. 5�      #   MegaMan.LevelEditor.StageForm  6)   5�      5�      �� 5�      $��Tiling��5�   "   .��5�      �� HI6    5�      �� 5�      $��Entities��5�   "   5�      5�      5�      5�      