import { DoGet } from '@/helpers/webFetchHelper';
import { Container, Grid, Paper, Table, Text } from '@mantine/core';
import { GetServerSideProps } from 'next';
import { useEffect, useState } from 'react';

interface SubathonProps {
    data: GetSubathonLeaderboardsDto | null;
    timeLeft: GetSubathonTimeLeftDto;
}


interface GetSubathonLeaderboardsDto {
    subsLeaderboard: Leaderboard[];
    bitsLeaderboard: Leaderboard[];
}

interface GetSubathonTimeLeftDto {
    timeLeft: string;
    playSound: boolean;
}

export interface Leaderboard {
    username: string;
    subsGifted: number;
    bitsDonated: number;
}

const Subathon = (props: SubathonProps) => {
    const [timeLeft, setTimeLeft] = useState(props.timeLeft);
    const [leaderboardData, setLeaderboardData] = useState(props.data);

    useEffect(() => {
        setInterval(async () => {
            try {
                const timeLeftApiRes = await DoGet('/api/Subathon/GetSubathonTimeLeft');
        
                if (timeLeftApiRes.ok) {
                    setTimeLeft(await timeLeftApiRes.json());
                }
                else {
                    setTimeLeft({
                        timeLeft: "Error getting time - " + timeLeftApiRes.status,
                        playSound: false
                    });
                }
        
            } catch (error) {
                setTimeLeft({
                    timeLeft: "Error getting time - " + error,
                    playSound: false
                });
            }
        }, 3000);

        setInterval(async () => {
            try {
                const leaderboardApiRes = await DoGet('/api/Subathon/GetSubathonLeaderboards');

                if (leaderboardApiRes.ok) {
                    setLeaderboardData(await leaderboardApiRes.json());
                }
                else {
                    console.error(leaderboardApiRes.status);
                    setLeaderboardData(null);
                }
            } catch (error) {
                console.error(error);
                setLeaderboardData(null);
            }
        }, 60000);
    }, []);

    return (
        <>
            {timeLeft.playSound &&
                <audio autoPlay>
                    <source src="/quack.mp3" type="audio/mpeg" />
                </audio>
            }

            <Text size={60} weight={500} align='center'>Subathon</Text>
            <Text size={30} weight={400} align='center' pt={20}>The subathon will end in...</Text>
            <Text size={30} weight={400} align='center' pt={20}>{timeLeft.timeLeft}</Text>

            <Text size={35} weight={500} align='center' pt={50} pb={20}>Top 10 Gifters/Cheerers</Text>

            <Container size='xl'>
                <Grid>
                    <Grid.Col span={6}>
                        <Text size={30} weight={400} align='center' pt={20} pb={20}>Gifted</Text>
                        <Paper shadow="md" radius="lg" p="md" sx={(theme) => ({
                            backgroundColor: theme.colors.dark[6]
                        })}>
                            <Table>
                                <thead>
                                    <tr>
                                        <th>Username</th>
                                        <th>Amount</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {leaderboardData && leaderboardData.subsLeaderboard.map((user) => (
                                        <tr key={user.username}>
                                            <td>
                                                {user.username}
                                            </td>
                                            <td>
                                                {user.subsGifted}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </Paper>
                    </Grid.Col>
                    <Grid.Col span={6}>
                        <Text size={30} weight={400} align='center' pt={20} pb={20}>Cheers</Text>
                        <Paper shadow="md" radius="lg" p="md" sx={(theme) => ({
                            backgroundColor: theme.colors.dark[6]
                        })}>
                            <Table>
                                <thead>
                                    <tr>
                                        <th>Username</th>
                                        <th>Amount</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {leaderboardData && leaderboardData.bitsLeaderboard.map((user) => (
                                        <tr key={user.username}>
                                            <td>
                                                {user.username}
                                            </td>
                                            <td>
                                                {user.bitsDonated}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </Paper>
                    </Grid.Col>
                </Grid>
            </Container>

        </>
    );
}

export const getServerSideProps: GetServerSideProps<SubathonProps> = async () => {
    let propsRes: SubathonProps = {
        timeLeft: {
            timeLeft: "",
            playSound: false
        },
        data: null
    };

    try {
        const timeLeftApiRes = await DoGet('/api/Subathon/GetSubathonTimeLeft');

        if (timeLeftApiRes.ok) {
            propsRes.timeLeft = await timeLeftApiRes.json();
        }
        else {
            propsRes.timeLeft = {
                timeLeft: "Error getting time - " + timeLeftApiRes.status,
                playSound: false
            };
        }

    } catch (error) {
        propsRes.timeLeft = {
            timeLeft: "Error getting time - " + error,
            playSound: false
        };
    }

    try {
        const leaderboardApiRes = await DoGet('/api/Subathon/GetSubathonLeaderboards');

        if (leaderboardApiRes.ok) {
            propsRes.data = await leaderboardApiRes.json();
        }
        else {
            propsRes.data = null;
        }
    } catch (error) {
        propsRes.data = null;
    }

    return {
        props: propsRes
    };
}

export default Subathon;