﻿#Region "Microsoft.VisualBasic::43d943823ceb23d7352a6828615f752c, analysis\SequenceToolkit\MSA\ScoreMatrix.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

' Class ScoreMatrix
' 
'     Sub: New
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Default

Public Class ScoreMatrix

    Public Matrix As Char()()

    Sub New(filePath$)
        Matrix = filePath _
            .ReadAllLines _
            .Select(Function(l) l.Replace(" "c, "").ToArray) _
            .ToArray
    End Sub

    Private Sub New()
    End Sub

    Public Shared Function DefaultMatrix() As DefaultValue(Of ScoreMatrix)
        Return New ScoreMatrix With {
            .Matrix = My.Resources _
                .Matrix _
                .lTokens _
                .Select(Function(l)
                            Return l.Replace(" "c, "").ToArray
                        End Function) _
                .ToArray
        }
    End Function
End Class
