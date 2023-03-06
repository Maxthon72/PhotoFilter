;Autor: Maciej Maciejewski gr.4
;Gliwice, AEiI, Informatyka 22/23
;J�zyki asemblerowe sem 5
;Temat: filtr do zdj��
;Program: Negative

;Program ten sluzy do przetwarzania tablicy bajt�w (Tab.1) 
;reprezentuj�cej obraz w formacie RGB na tablic�
;bajt�w (Tab.2) reprezentuj�c� ten sam obraz co Tab.1 
;tylko �e w negatywie. Ka�da warto�� RGB jest odejmowana od 255 
;dzi�ki czemu staje si� jej odwrotno�ci�
;Aby program dzia�a� poprwanie nale�y do niego przes�a� wska�nika 
;na Tab.1, wska�nik na Tab.2, wysoko�� oraz stride (padding) Tab.1
;Program nic nie zwraca wynik jest od razu zapisywany pod adresem 
;wskazanym przez wska�nik na Tab.2

;												RCX			     RDX		  R8		R9
; unsafe private static extern void GrayscaleAsm(byte* begining, byte* output, int hight, int Instride)
;
.data
white byte 255
.code
NegativeASM proc
        
        ;align 16

        push rax
        push r15
        push rdx
        mov rax, r8
        mul r9
        mov r14,rax

        xor r15, r15
        xor rax, rax
        pop rdx
        
        vpbroadcastb  xmm5,byte ptr[white]

@@:     
        movups xmm0,[RCX + r15]
        
        vpsubb  xmm0,xmm5,xmm0

        movups [rdx + r15],xmm0
        add r15, 16
        cmp r15,r14
        jl @B

Done:   vzeroupper
        
        pop r15
        pop rax
        ret
NegativeASM endp
end




