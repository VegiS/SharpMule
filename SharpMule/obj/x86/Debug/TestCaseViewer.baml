   M S B A M L   `   `   `  ����  	SharpMule     TestManager.TestCaseViewer   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  TestCaseViewer��5   	   $	��591��6    $	��893��6-    � WindowStyle$ SingleBorderWindow=�6R   .��5      � .+�5   
   �� -   5        $��tcTestViewer��5   d   $��0q�6   $	��524��6)   $
��Left=�66   $��	10,16,0,0q�6Q   $	��Top=�6x   $	��852��6�   .@�5      �� -   5        $��tabItemView��5   <   $
��View��6.   $��14(�6O   $
��Bold"�6]    � DataContext 6   �� .��5      &   System.Windows.Controls.DataGrid  -   5        $��dgTestViewer��5   �      AutoGenerateColumns . $��0q�66      CanUserSortColumns .  $	��478��6e      HorizontalGridLinesBrush �	#66000000   
IsReadOnly .    VerticalGridLinesBrush �	#69000000$	��848��6�   $��Consolas)�6J  5	      �� 5
      -   5        $��tabItemEdit��5
   $   $
��Edit��6   $y�Hidden=�67   $��14(�6K   $
��Bold"�6Y   .��5      �� -   5        $��tcScriptView��5      $y�Visible=�61   $	��489��6F   $	��848��6S   .@�5      �� -   5        $��tabMain��5   /   $��main.tc��6   .��5      ` \ICSharpCode.AvalonEdit, Version=4.1.0.7916, Culture=neutral, PublicKeyToken=9cc39be672370310'  !ICSharpCode.AvalonEdit.TextEditor  ?
avalonEdit.http://icsharpcode.net/sharpdevelop/avalonedit  60   -   5        $��txtScriptEditor��5   r   $��Consolas)�6�      SyntaxHighlightingM  GICSharpCode.AvalonEdit.Highlighting.HighlightingDefinitionTypeConverter$ C# 6�   $
��10pt(�6�   $	��448��6�   $	��832��6�   5      5      5      5      �� 5   
   -	   5        $��
btnRunSave��5   Z   $	��Run��6   $��26��6   $
��Left=�6+   $��
778,19,0,0q�6F   $	��Top=�6l   $��84��6�   6�   �� 5   
   -
   5        $��lblTestName��5   K   $��33��6   $
��Left=�6   $��
157,16,0,0q�67   $	��Top=�6^   $	��531��6v   $��18(�6�   $
��Bold"�6�   $��Italic$�6�   5      5      