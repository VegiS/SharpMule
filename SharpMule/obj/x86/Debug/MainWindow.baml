   M S B A M L   `   `   `  ����|  xSystem.Windows.Controls.DataVisualization.Toolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35�{clr-namespace:System.Windows.Controls.DataVisualization.Charting;assembly=System.Windows.Controls.DataVisualization.Toolkit2System.Windows.Controls.DataVisualization.Charting  5   	    	SharpMule   TestManager.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   ;toolkit-http://schemas.microsoft.com/wpf/2008/toolkit  5   	   �charting{clr-namespace:System.Windows.Controls.DataVisualization.Charting;assembly=System.Windows.Controls.DataVisualization.Toolkit  5   	   -   5        $��winTestManager��5   v     � Title$  	Test Mule��6	   $	��837��6   $
��1124��6(   $
��Bold"�65   $��14(�6G    � WindowStyle$ SingleBorderWindow=�6U    � WindowStartupLocation$ Manual=�6�   
������� J� 	AllowDrop .  .��5      � -   5        $��mainGrid��5      $	��822��6   $
��Left=�6(   $
��1109��6>    � ColumnDefinitions 5   
   �� 5	      $��1112* �6   �� 5
      $��8* �6   .+�5   
   � -   5        $��menuGrid��5      $��2��6   $��0,3,7,0q�63   .+�5      o� -   5        $��menuMain��5   :   $��22��6   $
��Left=�6   $	��Top=�6J   $
��1108��6b   $��	-5,0,-1,0q�6o   $y�Hidden=�6�   .@�5      m� -   5        $��	miOptions��5   ,   $��Options��6   $y�Hidden=�6=   .@�5      m� -   5        $��miEnableEmail��5   B   $��Enable Email Notification��6    m� IsCheckable . 6�   5      m� 5      -   5        $��miTools��5   *   $��Tools��6   $y�Hidden=�69   5      m� 5      -   5        $
��Help��5      $y�Hidden=�6@   5      5      � 5   
   -	   5        $��	prbStatus��5   =   $��46��6   $
��Left=�6"   $	��Top=�6N   $
��1109��6f   ���	#FFEC0303$��Bottom=�6�   $��2��6�   ��5      �� 5       �� EndPoint$	 0,1&�6&    �� 
StartPoint$	 0,0&�65   .��5       �  � Color$ 	#FFB2B2B2��6#   	 � Offset$	 0Q�65   � 5!      $ 	#FFFFFEFE��6#   $	 1Q�65   5"      5$      (� 5&   
   -
   5        $��viewBox��5&   $   
 (� Stretch$
 Uniform=�6   $��0,28,1,0q�63   $��2��6E   .��5'      �� .+�5(      � -   5        $��contentGrid��5(      $��2��6*   $	��860��6>    5)      �� 5*      $��403.094 �6+   �� 5+      $��125.906 �6+   �� 5,      $��43 �6+   �� 5-      $��40 �6+   �� 5.      $��29 �6+   �� 5/      $��283.994 �6+   �� 50      $��100.006 �6+   �� 51      $	��202 �6+   �� 52      $
��Auto �6+   �� 53      $
��Auto �6+   �� 54      $
��Auto �6+   .+�57      R� -   5         R�Name$ tvNavigation��57   L   $
��Left=�6   $��0,0,0,32q�6:   $	��457��6b   $��0q�6�   $	��770��6�   $��Bottom=�6
  $��12(�6%  $��Normal"�63  $��Euphemia)�6_  $��2��6u  ���	#FF382A74���	#FF643EB6 � DataContext 6G  ��  � ContextMenu 58      �� 59      -   5         ��Name$ tvContextMenu��59   *    .  ��6S   �� ��5;      �� 5<      $ 0.5,1&�62   $ 0.5,0&�6C   .��5=   "   � $ 
WhiteSmoke��6/   � 5>   "   $ 	#FFA0B4EA��6/   $	 1Q�6A   5?      .@�5A      �� $��7��6    $	��Top=�6+   $	��100��6C   5B      �� 5C      -   5        $��tabControl1��5C   \   $	��804��6!   $
��Left=�6.   $��	58,17,0,0q�6I   $	��Top=�6o   $	��762��6�   $��0q�6�   
�������$��7��6�   $��1��6�   .@�5D      �� -   5        $��tabItemView��5D   0   $
��View��6"   
������� 6C   �� .��5E      &   System.Windows.Controls.DataGrid  -   5        $��	dataGrid1��5E   ]   $��14(�6'      AutoGenerateColumns . $	��760��6P   $	��741��6n      HorizontalGridLinesBrush �	#66000000   VerticalGridLinesBrush �	#69000000   
IsReadOnly .    CanUserSortColumns .  $��0q�6S  $��Normal"�6g  $��Consolas)�6{  ���	#FF111010   AlternatingRowBackground �	#FFDEE8F3   AreRowDetailsFrozen .  6�  �� ��5F   "   �� 5G   &   $ 0.5,1&�6:   $ 0.5,0&�6K   .��5H   *   � $ 	#FFD8D7D7��67   $	 0.503Q�6I   � 5I   *   $ 	#FFF4F5F5��67   $	 0.997Q�6I   5J   '   5L      5M      �� 5N      -   5        $��tabItemEdit��5N   0   $
��Edit��6"   $y�	Collapsed=�6C   
�������.��5O      �� -   5        $��tcScriptView��5O   )   $y�Hidden=�6=   $	��760��6Q   $	��747��6^   .@�5P   "   �� -   5        $��tabMain��5P   8   $
��main��6*   .��5Q   &   ` \ICSharpCode.AvalonEdit, Version=4.1.0.7916, Culture=neutral, PublicKeyToken=9cc39be672370310'  !ICSharpCode.AvalonEdit.TextEditor  ?
avalonEdit.http://icsharpcode.net/sharpdevelop/avalonedit  6<   -   5        $��txtScriptEditor��5Q   ~   $��Consolas)�6�      SyntaxHighlightingM  GICSharpCode.AvalonEdit.Highlighting.HighlightingDefinitionTypeConverter$ C# 6�   $
��10pt(�6�   $	��750��6�   $	��736��6�   "   VerticalScrollBarVisibility$
 Auto=�6�      Text. 6   5R   *   �� 5S   .   -   5        $��cmScriptEditor��5S   :   5T   /   . 5U   M   5V   '   5X   #   5Y      5Z      �� 5[      -   5         ��Name$ tabItemHistory��5[   3   $��History��6"   ���	#FF422C12$��27��6b   $	��Top=�6n   ��5\      �� 5]   "   $	 0,1&�66   $	 0,0&�6E   .��5^   &   � $ 	#FFF3F3F3��63   $	 0Q�6E   � 5_   &   $ 	#FFEBEBEB��63   $		 0.5Q�6E   � 5`   &   $ 	#FFDDDDDD��63   $		 0.5Q�6E   � 5a   &   $ 	#FF171600��63   $	 1Q�6E   5b   #   .��5e      � -   5         �Name$ History��5e   #   $��
0,-250,0,0q�64   $	��752��6H   $	��761��6T    5f   "   �� 5g   &   $
��331* �67   �� 5h   &   $	��19* �67   �� 5i   &   $	��26* �67   .+�5k   "     -   5          Name$ 	dgHistory��5k   ^   $	��Top=�6+   $
��Left=�6C   $
��auto��6q   $
��auto��6   $��Normal"�6�   $��Calibri)�6�   $��14(�6�    .  .  5l   &   �� 5m   *   $ 0.5,1&�6>   $ 0.5,0&�6O   .��5n   .   � $ 	#FFCCD5DC��6;   $	 1Q�6M   � 5o   .   $ 	#FFC6E0F5��6;   $	 1Q�6M   5p   +   5r   #   >   8System.Windows.Controls.DataVisualization.Charting.Chart  5s   "   -   5          Name$ chartB��5s   1   $��0,350,-3,-254q�6A   $��0q�6X    J� RenderTransformOrigin$	 3,3&�6l   $
��Left=�6�   $	��755��6�   $��3��6�   
���   �   Series. 5t   &   E   ?System.Windows.Controls.DataVisualization.Charting.ColumnSeries  $��0q�6<   ���	#FF1AFB03H   BSystem.Windows.Controls.DataVisualization.Charting.DataPointSeries   DependentValuePath$ Value��6g   ���	#FF1AFB03$��Right=�6�       IndependentValuePath$	  Key��6�   !   IsSelectionEnabled! . $��0,0,-1,3q�6  $ 0.631,0.502&�6  ?   9System.Windows.Controls.DataVisualization.Charting.Series"   Title$" Test Stats (Past 30 days)��68  $	��509��6j  #   ItemsSource# 6�   �� 6j  5u   #   5v      5x      5z      �� 5|      -   5        $��
btnRunSave��5|   e   $	��Run��6   $��25��6+   $��Right=�67   $��0,17,8,0q�6S   $	��Top=�6w   $	��168��6�   
�������$��7��6�   
���  ��
���  ��$��14(�6�   �� 5}      -   5        $��cmbProjects��5}   Y   $��24��6   $
��Left=�6+   $��	90,18,0,0q�6F   $	��Top=�6l   $	��359��6�   $ �� SelectedIndex$$ 0��6�   $��12(�6�   
�������$��2��6  ��6�   �� 6  �� 5~      -   5        $��lblProjects��5~   n   $��Select Project��6   $��27��65   $
��Left=�6A   $��5,18,0,0q�6\   $	��Top=�6�   $��85��6�   $��12(�6�   �� 5      -   5        $��cbRerun��5   w   $��Run failed only��6   $��19��69   $
��Left=�6E   $��24.006,21,0,0q�6`   $	��Top=�6�   $��97��6�   {�. $��10(�6�   $��6��6   $��2��6  �� 5�      -   5        $��lblTestName��5�   X   $��29��6   $
��Left=�6(   $��480,668,0,0q�6C   $	��Top=�6k   $	��722��6�   $��Normal$�6�   $��14(�6�   $��Center=�6�   $��Center=�6�   5�      5�      5�      �� 5�   
   -   5        $��lblTestNameProgress��5�   z   $�� ��6   $
��Left=�6   $��
425,19,0,0q�66   $	��Top=�6J   $��24��6b   $	��307��6n   6z   �� 5�   
   -    5        % ��Name$% rtbErrorMessage��5�   *   $y�Hidden=�6   $
��Left=�6C   $	��764��6^   $��0,48,0,0q�6k   $	��Top=�6}   $
��1099��6�   . �5�      /� .T�5�      J� .*�6   .*�5�      �� $2�,Test Mule Was unable to start successfuly...��6   5�      J� 5�      .*�6   .*�5�      �� $U�OThis usually happens if youre configuration file is missing or has invalid data��6   5�      J� 5�      .*�6   .*�5�      �� $p�jVerify that Properites directory exists and its sibling relationship to the SharpMule.exe Parent directory��6   5�      J� 5�      .*�6   .*�5�      �� $���Verify configuration.xml file projects node contains corrects paths to all of the projects. Also double check Environments/Settings/Root node as well��6   5�      J� 5�      .*�6   .*�5�      �� $�Error Message��6   5�      J� 5�      .*�6   .*�5�      �� $� ��6   5�      5�      5�      5�      5�      