import { DoDelete } from "@/helpers/webFetchHelper";
import { AppShell, Burger, Button, createStyles, Grid, Group, Header, MediaQuery, Navbar, NavLink, rem } from "@mantine/core";
import { signOut, useSession } from "next-auth/react";
import { AppProps } from "next/app";
import Link from "next/link";
import { useEffect, useState } from "react";
import { toast } from "react-toastify";

const useStyles = createStyles((theme) => ({
    header: {
        backgroundColor: theme.colors.dark[6],
        paddingBottom: 0,
    },
    navbar: {
        backgroundColor: theme.colors.dark[6],
        paddingBottom: 0,
    },
    mainLinks: {
        fontWeight: "bold",
        '&:hover': {
            backgroundColor: theme.colors.dark[5],
            color: theme.colorScheme === 'dark' ? theme.white : theme.black
        }
    },
    subLinks: {
        '&:hover': {
            backgroundColor: theme.colors.dark[5],
            color: theme.colorScheme === 'dark' ? theme.white : theme.black
        }
    }
}));

const Navigation = (props: AppProps) => {
    const { Component, pageProps } = props;
    const { classes } = useStyles();
    const [burgerOpened, setBurgerOpened] = useState(false);
    const [windowPathName, setWindowPathName] = useState("");

    useEffect(() => {
        setWindowPathName(window.location.pathname);
    }, [])

    return (
        <>
            <AppShell
                header={
                    <Header height={60} className={classes.header}>
                        <Grid>
                            <Grid.Col span={6}>
                                { /* do not display when it's its larger than sm */}
                                <MediaQuery largerThan="sm" styles={{ display: 'none' }}>
                                    <Burger
                                        opened={burgerOpened}
                                        onClick={() => setBurgerOpened((o) => !o)}
                                        size="sm"
                                        mr="xl"
                                        style={{ marginTop: 15, marginRight: 20 }}
                                    />
                                </MediaQuery>
                            </Grid.Col>
                            <Grid.Col span={6} sx={{ display: 'flex', justifyContent: 'right' }}>
                            </Grid.Col>
                        </Grid>
                    </Header>
                }
                navbar={
                    <Navbar hiddenBreakpoint="sm" hidden={!burgerOpened} width={{ sm: 150, lg: 150 }} className={classes.navbar}>
                        <Navbar.Section>
                            <Link href='/' passHref style={{ textDecoration: 'none' }}>
                                <NavLink label='Home' className={classes.mainLinks} active={'/' === windowPathName} onClick={() => setWindowPathName('/')} />
                            </Link>
                            <Link href='/commands' passHref style={{ textDecoration: 'none' }}>
                                <NavLink label='Commands' className={classes.mainLinks} active={'/commands' === windowPathName} onClick={() => setWindowPathName('/commands')} />
                            </Link>
                            <Link href='/subathon' passHref style={{ textDecoration: 'none' }}>
                                <NavLink label='Subathon' className={classes.mainLinks} active={'/subathon' === windowPathName} onClick={() => setWindowPathName('/subathon')} />
                            </Link>
                            <NavLink label='Leaderboards' className={classes.mainLinks}>
                                <Link href='/leaderboards/0' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Points' className={classes.mainLinks} active={'/leaderboards/0' === windowPathName} onClick={() => setWindowPathName('/leaderboards/0')} />
                                </Link>
                                <Link href='/leaderboards/1' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='All Time Hours' className={classes.mainLinks} active={'/leaderboards/1' === windowPathName} onClick={() => setWindowPathName('/leaderboards/1')} />
                                </Link>
                                <Link href='/leaderboards/2' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Stream Hours' className={classes.mainLinks} active={'/leaderboards/2' === windowPathName} onClick={() => setWindowPathName('/leaderboards/2')} />
                                </Link>
                                <Link href='/leaderboards/3' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Weekly Hours' className={classes.mainLinks} active={'/leaderboards/3' === windowPathName} onClick={() => setWindowPathName('/leaderboards/3')} />
                                </Link>
                                <Link href='/leaderboards/4' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Monthly Hours' className={classes.mainLinks} active={'/leaderboards/4' === windowPathName} onClick={() => setWindowPathName('/leaderboards/4')} />
                                </Link>
                                <Link href='/leaderboards/5' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Points Won' className={classes.mainLinks} active={'/leaderboards/5' === windowPathName} onClick={() => setWindowPathName('/leaderboards/5')} />
                                </Link>
                                <Link href='/leaderboards/6' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Points Lost' className={classes.mainLinks} active={'/leaderboards/6' === windowPathName} onClick={() => setWindowPathName('/leaderboards/6')} />
                                </Link>
                                <Link href='/leaderboards/7' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Points Gambled' className={classes.mainLinks} active={'/leaderboards/7' === windowPathName} onClick={() => setWindowPathName('/leaderboards/7')} />
                                </Link>
                                <Link href='/leaderboards/8' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Total Spins' className={classes.mainLinks} active={'/leaderboards/8' === windowPathName} onClick={() => setWindowPathName('/leaderboards/8')} />
                                </Link>
                                <Link href='/leaderboards/9' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Current Streak' className={classes.mainLinks} active={'/leaderboards/9' === windowPathName} onClick={() => setWindowPathName('/leaderboards/9')} />
                                </Link>
                                <Link href='/leaderboards/10' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Highest Streak' className={classes.mainLinks} active={'/leaderboards/10' === windowPathName} onClick={() => setWindowPathName('/leaderboards/10')} />
                                </Link>
                                <Link href='/leaderboards/11' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Total Claims' className={classes.mainLinks} active={'/leaderboards/11' === windowPathName} onClick={() => setWindowPathName('/leaderboards/11')} />
                                </Link>
                                <Link href='/leaderboards/12' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Marbles Won' className={classes.mainLinks} active={'/leaderboards/12' === windowPathName} onClick={() => setWindowPathName('/leaderboards/12')} />
                                </Link>
                                <Link href='/leaderboards/13' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Discord Streak' className={classes.mainLinks} active={'/leaderboards/13' === windowPathName} onClick={() => setWindowPathName('/leaderboards/13')} />
                                </Link>
                                <Link href='/leaderboards/14' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Discord Level' className={classes.mainLinks} active={'/leaderboards/14' === windowPathName} onClick={() => setWindowPathName('/leaderboards/14')} />
                                </Link>
                                <Link href='/leaderboards/15' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Discord XP' className={classes.mainLinks} active={'/leaderboards/15' === windowPathName} onClick={() => setWindowPathName('/leaderboards/15')} />
                                </Link>
                                <Link href='/leaderboards/16' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Discord Daily Claims' className={classes.mainLinks} active={'/leaderboards/16' === windowPathName} onClick={() => setWindowPathName('/leaderboards/16')} />
                                </Link>
                                <Link href='/leaderboards/17' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Messages Sent' className={classes.mainLinks} active={'/leaderboards/17' === windowPathName} onClick={() => setWindowPathName('/leaderboards/17')} />
                                </Link>
                                <Link href='/leaderboards/18' passHref style={{ textDecoration: 'none' }}>
                                    <NavLink label='Twitch Bosses Survived' className={classes.mainLinks} active={'/leaderboards/18' === windowPathName} onClick={() => setWindowPathName('/leaderboards/18')} />
                                </Link>
                            </NavLink>
                        </Navbar.Section>
                    </Navbar>
                }
            >
                <Component {...pageProps} />
                {/* modals*/}
            </AppShell>
        </>
    );
}

export default Navigation;