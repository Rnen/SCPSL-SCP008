# SCP008

- Zombier (SCP-049-2) vil infisere spillere med SCP-008.
- De vil stadig ta skade til døden eller de helbreder seg selv med et helse-sett.
- Når spilleren dør av SCP-008-effekten, blir de selv en zombie (SCP-049-2)

> Krever [SMod](https://github.com/Grover-c13/Smod2/)

> Slik installerer du: Gå til [Utgivelser](https://github.com/Rnen/SCP008/releases) og velg riktig versjon. Slipp det i plugins-mappen

## Konfigurasjonsalternativer

Konfigurasjonsnøkkel | Standardverdi | Beskrivelse
--: | :-: | :--
SCP008_enabled | True | Aktiverer / deaktiverer plugin-funksjonalitet
SCP008_damage_amount | 1 | Hvor mye den skader per intervall
SCP008_damage_interval | 2 | Hvor ofte den skader (i sekunder)
SCP008_swing_damage | 0 | Hvor mye 049-2s angrep handler om (0 eller lavere vil bruke standardverdi)
scp008_zombiekill_infects | False | Hvis vanlige drap av zombier bør forvandle spillere
scp008_infect_chance | 100 | Hvor stor sjanse er det for infeksjon når du blir truffet
scp008_cure_enabled | True | Hvis du kan kurere infeksjon med helse-sett
scp008_cure_chance | 100 | Hvor stor sjanse for å bli helbredet
scp008_ranklist_commands |  | Hvilke serverrangeringer kan bruke plugin-kommandoene (fungerer som en sekundær hviteliste)
scp008_roles_caninfect | -1 | Hvilke spillroller kan bli infisert (-1 er alle roller)
scp008_canhit_tutorial | true | Om zombier kan treffe opplæringsspillere eller ikke
scp008_anyDeath | false | Hvis noen dødsfall etter infeksjon skulle forvandle spillere
scp008_assist079_experience | 35f | SCP 079 EXP mottatt for assists
scp008_broadcast | true | Spillere mottar en personlig sending ved infeksjon og transformasjon
scp008_broadcast_duration | 7 | Varighet av den personlige sendingen ovenfor

## Kommandoer

Kommando | Argumenter (hvis noen) | Beskrivelse
--: | :-: | :--
scp008 / scp8 |  | Aktiverer / deaktiverer plugin-funksjonalitet
infect | Spillernavn / ID / Steam | Infiserer / helbreder spiller
