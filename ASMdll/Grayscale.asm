;Autor: Maciej Maciejewski gr.4
;Gliwice, AEiI, Informatyka 22/23
;Jêzyki asemblerowe sem 5
;Temat: filtr do zdjêæ
;Program s³u¿y do przetworzenia wektora bitów o d³ugoœci równej width podanej przez urzytkownika
;odpowiadaj¹cego szeregowi wartoœci RGB na szereg wartoœci w grayscalu
;Program w pierwszych krokach odpowiednio ustawia wartoœæ tak aby powsta³y rejestry gdzie s¹ same R, G lub B wartoœci
;Nastêpnie wartoœci te s¹ mnorzone przez odpowiednie wspó³czynniki, dodawana do siebie i zapisywane jako kolekcjia koñcowa
;Aby program dzia³a poprawnie nale¿y do niego przes³aæ wskaŸnik na kolekcjiê pocz¹tkow¹ i koñcowo¹ oraz d³ugoœæ kolekcji
;Program nic nie zwraca wynik jest od razu zapisywany pod adresem wskazanym przez wskaŸaŸnik podany przez urzytkownika

;
;													RCX			  RDX		  R8		
; unsafe private static extern void GrayscaleAsm(byte* pCur, byte* output, int width);
.data
rMulti		real4   0.2126
gMulti		real4	0.7152
bMulti		real4	0.0722
white       real4	255.0


.code
GrayscaleASM proc
        align 16

        push rax
        push r15

        vbroadcastss ymm10,real4 ptr [rMulti]       ;ymm10 = packed 0.2126
        vbroadcastss ymm11,real4 ptr [gMulti]       ;ymm11 = packed 0.7152
        vbroadcastss ymm12,real4 ptr [bMulti]       ;ymm12 = packed 0.0722
        vbroadcastss ymm13,real4 ptr [white]        ;ymm13 = packed 255.0
        vxorps ymm15,ymm14,ymm15                    ;ymm14 = packed 0.0
        xor r15, r15
        xor r11, r11
        xor rax, rax
        mov r8d,r8d                             ;r8 = num_pixels
        xor r12, r12
        mov r10,8                               ;r10 = number of pixels / iteration

@@:     pmovzxbd xmm0,dword ptr [rcx+r15]   ;rbgr
        add r15, 4
        pmovzxbd xmm1,dword ptr [rcx+r15]   ;grbg
        add r15, 4
        pmovzxbd xmm2,dword ptr [rcx+r15]	;bgrb
        add r15, 4

        pmovzxbd xmm3,dword ptr [rcx+r15]   ;rbgr
        add r15, 4
        pmovzxbd xmm4,dword ptr [rcx+r15]   ;grbg
        add r15, 4
        pmovzxbd xmm5,dword ptr [rcx+r15]   ;bgrb
        add r15, 4

        vinsertf128  ymm0, ymm0, xmm3, 1	    ; ymm0 = m03 rbgrrbgr
        vinsertf128  ymm1, ymm1, xmm4, 1	    ; ymm1 = m14 grbggrbg
        vinsertf128  ymm2, ymm2, xmm5, 1	    ; ymm2 = m25 bgrbbgrb

        vshufps     ymm3,       ymm1,       ymm2,   10011110b	    ; ymm3 = xy (shuffle m14(ymm1), m25(ymm2)) gr
;                grgr grgr | grbg grbg | bgrb bgrb

        vshufps ymm4, ymm0, ymm1, 01001001b     ; ymm4 = yz (shuffle m03(ymm0), m14(ymm1)) bg
        vshufps ymm5, ymm0, ymm3, 10001100b	    ; ymm5 = x (shuffle m03, xy) r
        vshufps ymm6, ymm4, ymm3, 11011000b	    ; ymm6 = y (shuffle yz, xy) g
        vshufps ymm7, ymm4, ymm2, 11001101b     ; ymm7 = z (shuffle yz, m25) b

        vmovaps ymm0, ymm5;
        vmovaps ymm1, ymm6;
        vmovaps ymm2, ymm7;

        vcvtdq2ps ymm0,ymm0                     ;ymm0 = r values (Single Precision Floating Point)
        vcvtdq2ps ymm1,ymm1                     ;ymm1 = g values (SPFP)
        vcvtdq2ps ymm2,ymm2                     ;ymm2 = b values (SPFP)

        vmulps ymm0,ymm0,ymm10                  ;ymm0 = r * 0.2126
        vmulps ymm1,ymm1,ymm11                  ;ymm1 = g * 0.7152
        vmulps ymm2,ymm2,ymm12                  ;ymm2 = b * 0.0722

        vaddps ymm3,ymm0,ymm1                   ;r + g
        vaddps ymm4,ymm3,ymm2                   ;r + g + b       
        vminps ymm0,ymm4,ymm13                  ;clip pixels above 255.0
        vmaxps ymm1,ymm0,ymm14                  ;clip pixels below 0.0

        vcvtps2dq ymm2,ymm1                     ;convert values (SPFP) to dwords
        vpackusdw ymm3,ymm2,ymm2
        vextracti128 xmm4,ymm3,1

        vpackuswb xmm5,xmm3,xmm4                ;byte GS pixels in xmm5
        vpextrd r11d,xmm5,0                     ;r11d = 4 grayscale pixels
        mov dword ptr [rdx+rax],r11d            ;save 4 grayscale image pixels
        vpextrd r11d,xmm5,2                     ;r11d = 4 grayscale pixels
        mov dword ptr [rdx+rax+4],r11d          ;save 4 grayscale image pixels

        add rax,r10
        sub r8,r10
        cmp r12, r8
        jnge @B                                 ;jump to @@ label that is backward (up in code) if pixels left to process

Done:   vzeroupper
        pop r15
        pop rax
        
        ret
GrayscaleASM endp
end