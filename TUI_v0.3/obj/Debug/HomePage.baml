   M S B A M L   `   `   `  ����C  ?TUI_v0.3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:TUI_v0._3	TUI_v0._3  5      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5      3clr-namespace:TUI_v0._3.SlidesTUI_v0._3.Slides  5	          TUI_v0._3.HomePage   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      )localclr-namespace:TUI_v0._3     5      U QMaterialDesignThemes.Wpf, Version=2.3.0.823, Culture=neutral, PublicKeyToken=nullX TMaterialDesignThemes.MahApps, Version=0.0.11.0, Culture=neutral, PublicKeyToken=nullHmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes   5      5system&clr-namespace:System;assembly=mscorlib   5      -local1clr-namespace:TUI_v0._3.Slides   5	      -   5          >�Name$  window��5      .��5      � -   5         �Name$ 	main_grid��5       � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   �� 5      $��* �6   6)   .+�5   
   �� -   5         ��Name$ back_button��5      $��0��6&   $��0 0 15 0q�63   $��Right=�6E    K� ZIndex$ 10��6a    J� Opacity$ 0Q�6s   .��5      '  !MaterialDesignThemes.Wpf.PackIcon  �  �ControlzEx.PackIconBase`1[[MaterialDesignThemes.Wpf.PackIconKind, MaterialDesignThemes.Wpf, Version=2.3.0.823, Culture=neutral, PublicKeyToken=null]]   Kind$ 	Backspace=�6&   69   5      (  "MaterialDesignThemes.Wpf.ColorZone  5   
      Mode$
 Dark=�6#   $��0��60   $��16q�6=   .��5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   66   �� 5      $��* �6'   63   .+�5       o� $��0��6   	 o� 
IsMainMenu	 . $	��Top=�69   $	��Top=�6N   $
��Left=�6f   .@�5!      m� -   5        
 m�Name$
 
FileButton��5!      $��32��63   $��32��6>   .@�5"      m� $��Створити��6#   
�������.@�5$      m� -   5        $%��Електрична схема��5$   '   
�������5%   ;   m� 5&      -   5        $)��#Лабораторна робота��5&   '   
�������6V   5'      m� 5(      $��Відкрити��6#   
�������.@�5)      m� -   5        $#��Існуючий проект��5)   '   
�������6S   m� 5*      -   5        $)��#Лабораторна робота��5*   '   
�������6V   5+      m� 5,      -	   5        $9��3Зчитування даних з приладів��5,   #   
�������{�. 6l   m� 5-      -
   5        $)��#Розпізнавання схем��5-   #   
�������{�. 6c   �� 5.      m� 5/      $��Зберегти��6#   
�������{�.   m� Icon 50        51   "   $ ContentSave=�6:   53      m� 54      $��Зберегти як..��6#   
�������{�.  6M   �� 55      m� 56      -   5        $��Про PhysHelper��56   #   
�������6N   �� 57      m� 58      -   5        $��
Вийти��58   #    m� InputGestureText$ Ctrl+E��62   
������� 59        5:   "   $ 	ExitToApp=�6:   
�������6K   5<      5=      5>      � 5?      $��1��6    5@      �� 5A      $
��Auto �6+   6:   �� 5B      $��* �6+   67   .+�5D      �� $��0��6    $��Center=�60   $��16 0 0 0q�6K    �� TextAlignment$ Center=�5E       $��DemiBold"�5F       $��18(�66   
Меню5H      5I      5J      5K      7  1MaterialDesignThemes.Wpf.Transitions.Transitioner  5M   
    �� SelectedIndex$ 0��6&   $��0 16 0 0q�68   $��1��6J   .@�5N      <  6MaterialDesignThemes.Wpf.Transitions.TransitionerSlide  .��5O         TUI_v0._3.Slide1  6!   5P        5Q      .��5R         TUI_v0._3.Slide2  6!   5S        5T      .��5U         TUI_v0._3.Slides.Slide4  6"   5V        5W      .��5X      	   TUI_v0._3.Slides.Slide5	  6"   5Y        5Z      .��5[      
   TUI_v0._3.Slides.Slide6
  6"   5\        5]      .��5^         TUI_v0._3.Slide3  6!   5_      5`      �� 5b   
    �� Orientation$ 
Horizontal=�6   $��Center=�6.   $��Center=�6I   $��2��6f   .+�5c      #  MaterialDesignThemes.Wpf.Card  $	��420��6"   $	��270��6.   +  %MaterialDesignThemes.Wpf.ShadowAssist   ShadowDepth$ Depth3=�6;   .��5d      � $��Stretch=�6    5e      �� 5f      $
��Auto �6(   �� 5g      $
��Auto �6(    5i      �� 5j      $	��200 �6+   �� 5k      $��* �6+   .+�5m      �� -   5         ��Name$ CircuitImage��5m   0    �� Stretch$
 Fill=�6    $	��160��6F   $	��160��6R   $��Center=�6_   $��Center=�6|   �� 5n      $��1��6!   .+�5o      ��  #  MaterialDesignHeadlineTextBlock#��[  6$   $��
16 16 16 8q�6]   $��18(�6q   $��DemiBold"�6   $"Ваш власний проект6�   �� 5p      $
��Wrap=�6$   $��	16 0 16 8q�68     MaterialDesignBodyLight#���  6K   $	��Top=�5q   ,   ��В цьому режимі ви можете створити та обрахувати будь-які електричні ланцюги.6�   5r      �� 5s      $��1��6   $��2��6*   $��0 1 0 0q�6>     MaterialDesignDivider#���  6X    �� Padding$ 8q�6�   ._�5t      ]� .+�5u      �� -   5        $ CreateCircuitButton��5w   -     MaterialDesignFlatButton#���  5u   %   $��Right=�5v   -   .��5x   "   �� $ 
Horizontal=�6-   .+�5y   &     $ PlusCircleOutline=�6>   $��Center=�6W   �� 5z   &   $��8 0 0 0q�60   $��Center=�6A   Створити6f   5{   #   5|      �� 5}      #���  6(   $
��Wrap=�6_   $��16q�6s   $��Center=�6   ][Бажаєте спробувати? Натисніть на кнопку праворуч!5      5�      5�      5�      5�        5�      $	��420��6"   $	��270��6.   $��8 0 0 0q�6;   $ Depth3=�6L   .��5�      � $��Stretch=�6    5�      �� 5�      $
��Auto �6(   �� 5�      $
��Auto �6(    5�      �� 5�      $	��200 �6+   �� 5�      $��* �6+   .+�5�      �� -   5        $ LabWorkImage��5�   0   $
 Fill=�6    $	��160��6F   $	��160��6R   $��Center=�6_   $��Center=�6|   �� 5�      $��1��6!   .+�5�      �� #��[  6$   $��
16 16 16 8q�6]   $��18(�6q   $��DemiBold"�6   %#Лабораторні роботи6�   �� 5�      $
��Wrap=�6$   $��	16 0 16 8q�68   #���  6K   $	��Top=�5�   ,   ywСпробуйте виконати наші роботи та освоїти базові поняття фізики.6�   5�      �� 5�      $��1��6   $��2��6*   $��0 1 0 0q�6>   #���  6X   $ 8q�6�   ._�5�      ]� .+�5�      �� -   5        $ CreateLabWorkButton��5�   -   #���  5�   %   $��Right=�5�   -   .��5�   "   �� $ 
Horizontal=�6-   .+�5�   &     $ PlusCircleOutline=�6>   $��Center=�6W   �� 5�   &   $��8 0 0 0q�60   $��Center=�6A   Відкрити6f   5�   #   5�      �� 5�      #���  6(   $
��Wrap=�6_   $��16q�6s   $��Center=�6   ][Бажаєте спробувати? Натисніть на кнопку праворуч!5�      5�      5�      5�      5�      5�      5�      5�      