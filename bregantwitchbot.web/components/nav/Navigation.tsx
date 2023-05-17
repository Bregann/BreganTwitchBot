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

    const LogoutUser = (sessionId: string) => {
        signOut();
        DoDelete('/api/auth/DeleteUserSession', sessionId);

        toast.success('Succesfully logged out', {
            position: 'bottom-right',
            closeOnClick: true,
            theme: 'colored'
        });
    }

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