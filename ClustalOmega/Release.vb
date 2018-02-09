﻿#Region "Microsoft.VisualBasic::71985201199540fe264c757f85b8b251, ..\GCModeller\analysis\SequenceToolkit\ClustalOmega\Release.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Language

Public Module Release

    ''' <summary>
    ''' Release the clustal program files from this assembly module resources data.
    ''' (将本模块资源数据之中的Clustal程序释放至目标文件夹之中)
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <returns>返回clustal程序的路径</returns>
    ''' <remarks></remarks>
    Public Function ReleasePackage(DIR As String) As String
        On Error Resume Next

        Call FileIO.FileSystem.CreateDirectory(DIR)

        With App.GetAppSysTempFile(".zip", App.PID)
            Call My.Resources.clustal_omega_1_2_2_win64.FlushStream(.ByRef)
            Call GZip.ImprovedExtractToDirectory(.ByRef, DIR, Overwrite.Always)
        End With

        Return DIR & "/clustalo.exe"
    End Function
End Module
